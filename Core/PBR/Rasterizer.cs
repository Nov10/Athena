using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renderer.Maths;
using Assimp;
using NPhotoshop.Core.Image;
using Microsoft.UI.Xaml.Media;
using Windows.UI.StartScreen;
using Renderer.Core.Shader;
using System.Reflection.Metadata.Ecma335;

namespace Renderer
{
    public struct Raster
    {
        public int x;
        public int y;
        public int TriangleIndex;
        public Vector3 Normal_WorldSpace;
        public Vector3 Lambda;
        public Vector3 Position_ScreenVolumeSpace;
        public Vector2 UV;

        public Raster(int x, int y, int triangleIndex, Vector3 normal_WorldSpace, Vector3 lambda, Vector3 position_ScreenVolumeSpace, Vector2  uv)
        {
            this.x = x;
            this.y = y;
            TriangleIndex = triangleIndex;
            Normal_WorldSpace = normal_WorldSpace;
            Lambda = lambda;
            UV = uv;
            Position_ScreenVolumeSpace = position_ScreenVolumeSpace;
        }
    }
    public class Rasterizer
    {
        const int tileSize = 4;
        const int MaxTCount = 32;

        public static void ConvertVertexToScreenSpace(Vertex[] vertexes, int width, int height)
        {
            Parallel.For(0, vertexes.Length, (idx) =>
            {
                vertexes[idx].Position_ScreenVolumeSpace.x = vertexes[idx].Position_ScreenVolumeSpace.x * (width / 2.0f) + (width / 2.0f);
                vertexes[idx].Position_ScreenVolumeSpace.y = vertexes[idx].Position_ScreenVolumeSpace.y * (height / 2.0f) + (height / 2.0f);
            });
        }

        private static TileCache tileCache;
        public Rasterizer(int width, int height)
        {
            int widthInTiles = width / tileSize;
            int heightInTiles = height / tileSize;
            int numTiles = widthInTiles * heightInTiles;
            Rasters = new Raster[width * height];

            tileCache = new TileCache(widthInTiles, heightInTiles);
        }
        private static float EdgeFunction(Vector3 a, Vector3 b, Vector3 c)
        {
            // 삼각형의 면적을 구하는 에지 함수
            return (c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x);
        }


        private static void SetPixelWithZBuffer(int x, int y, FragmentShader shader, int triangleIndex, Vertex p1, Vertex p2, Vertex p3, float[] ZBuffer, int width)
        {
            int idx = x + y * width;
            
            Vector3 p = new Vector3(x, y, 0);
            float lambda1 = EdgeFunction(p2.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace, p);
            float lambda2 = EdgeFunction(p3.Position_ScreenVolumeSpace, p1.Position_ScreenVolumeSpace, p);
            float lambda3 = EdgeFunction(p1.Position_ScreenVolumeSpace, p2.Position_ScreenVolumeSpace, p);

            if(lambda1 > 0 && lambda2 >0 && lambda3 >0)
            {
                //float areaABC = EdgeFunction(p1.Position_ScreenVolumeSpace, p2.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace);
                float areaABC = lambda1 + lambda2 + lambda3;
                float zInterpolated = (lambda1 * p1.Position_ScreenVolumeSpace.z + lambda2 * p2.Position_ScreenVolumeSpace.z + lambda3 * p3.Position_ScreenVolumeSpace.z) / areaABC;
                if (zInterpolated < ZBuffer[idx])
                {
                    ZBuffer[idx] = zInterpolated;
                    p.z = zInterpolated;

                    Rasters[idx].x = x;
                    Rasters[idx].y = y;
                    Rasters[idx].TriangleIndex = triangleIndex;
                    Rasters[idx].UV = (lambda1 * p1.UV + lambda2 * p2.UV + lambda3 * p3.UV) / (areaABC);
                    Rasters[idx].Lambda = new Vector3(lambda1, lambda2, lambda3);
                    Rasters[idx].Normal_WorldSpace = lambda1 * p1.Normal_WorldSpace + lambda2 * p2.Normal_WorldSpace + lambda3 * p3.Normal_WorldSpace;
                    Rasters[idx].Position_ScreenVolumeSpace = p;
                }
            }
        }

        public static void CalculateCache_TrianglesOnPerTile(
            int[] triangles, Vertex[] vertexes,
            int screenWidth,
            int screenHeight)
        {
           
            Parallel.For(0, triangles.Length/3, (idx) =>
            {
                Vertex p1 = vertexes[3 * idx];
                Vertex p2 = vertexes[3 * idx + 1];
                Vertex p3 = vertexes[3 * idx + 2];

                if (EdgeFunction(p1.Position_ScreenVolumeSpace, p2.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace) < 0)
                    return;

                if (0 < Vector3.Cross_Z(p2.Position_ScreenVolumeSpace - p1.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace - p1.Position_ScreenVolumeSpace))
                    return;

                int tileStartX = Math.Max((int)Math.Floor(Math.Min(p1.Position_ScreenVolumeSpace.x, Math.Min(p2.Position_ScreenVolumeSpace.x, p3.Position_ScreenVolumeSpace.x)) / tileSize), 0);
                int tileStartY = Math.Max((int)Math.Floor(Math.Min(p1.Position_ScreenVolumeSpace.y, Math.Min(p2.Position_ScreenVolumeSpace.y, p3.Position_ScreenVolumeSpace.y)) / tileSize), 0);
                int tileEndX = Math.Min((int)Math.Ceiling(Math.Max(p1.Position_ScreenVolumeSpace.x, Math.Max(p2.Position_ScreenVolumeSpace.x, p3.Position_ScreenVolumeSpace.x)) / tileSize), screenWidth / tileSize);
                int tileEndY = Math.Min((int)Math.Ceiling(Math.Max(p1.Position_ScreenVolumeSpace.y, Math.Max(p2.Position_ScreenVolumeSpace.y, p3.Position_ScreenVolumeSpace.y)) / tileSize), screenHeight / tileSize);

                for (int tileY = tileStartY; tileY < tileEndY; tileY++)
                {
                    for (int tileX = tileStartX; tileX < tileEndX; tileX++)
                    {
                        int tileIndex = tileY * (screenWidth / tileSize) + tileX;
                        lock (tileCache.perTileTriangleCountArray)
                        {
                            if (tileCache.perTileTriangleCountArray[tileIndex] < MaxTCount)
                            {
                                int linearIndex = tileIndex * MaxTCount + tileCache.perTileTriangleCountArray[tileIndex];
                                tileCache.perTileTriangleArray[linearIndex] = idx;
                                tileCache.perTileTriangleCountArray[tileIndex]++;
                            }
                        }
                    }
                }
            });
        }

        public static Raster[] TiledRasterizationKernel(
            FragmentShader shader,
            float[] zBuffer,
            Vertex[] vertexes,
            int screenWidth,
            int screenHeight)
        {
            int numTiles = screenWidth * screenHeight / (tileSize * tileSize);
            Parallel.For(0, numTiles, (idx) =>
            {
                // 타일 시작 자편 계산
                int tileX = (idx % (screenWidth / tileSize)) * tileSize;
                int tileY = (idx / (screenWidth / tileSize)) * tileSize;

                // 타일 끝 자편 계산 (화면 경계를 넘지 않도록)
                int tileEndX = Math.Min(tileX + tileSize, screenWidth);
                int tileEndY = Math.Min(tileY + tileSize, screenHeight);

                // 해당 타일에 포함된 삼각형 개수 가져오기
                int triangleCount = tileCache.perTileTriangleCountArray[idx];

                for (int y = tileY; y < tileEndY; y++)
                {
                    for (int x = tileX; x < tileEndX; x++)
                    {
                        for (int i = 0; i < triangleCount; i++)
                        {
                            int record = tileCache.perTileTriangleArray[idx * MaxTCount + i];

                            SetPixelWithZBuffer(x, y, shader, record, vertexes[3 * record], vertexes[3 * record + 1], vertexes[3 * record + 2], zBuffer, screenWidth);
                        }
                    }
                }
            });
            return Rasters;
        }
        readonly static FragmentShader Shader;
       static Raster[] Rasters;
        public Raster[] RunTiled(Vertex[] vertices, FragmentShader f, float[] zBuffer, NBitmap target, int[] triangles, int width, int height)
        {
            Parallel.For(0, Rasters.Length, (idx) =>
            {
                Rasters[idx].TriangleIndex = -1;
            });
            Parallel.For(0, tileCache.perTileTriangleCountArray.Length, (idx) =>
            {
                tileCache.perTileTriangleCountArray[idx] = 0;
            });

            ConvertVertexToScreenSpace(vertices, width, height);
            CalculateCache_TrianglesOnPerTile(triangles, vertices, width, height);
            TiledRasterizationKernel(f, zBuffer, vertices, width, height);
            return Rasters;
        }
        public class TileCache
        {
            public int widthInTiles;
            public int heightInTiles;
            public int[] perTileTriangleArray; //value : triangleIndex
            public int[] perTileTriangleCountArray;

            public TileCache(int widthInTiles, int heightInTiles)
            {
                this.widthInTiles = widthInTiles;
                this.heightInTiles = heightInTiles;

                perTileTriangleCountArray = new int[widthInTiles * heightInTiles];
                perTileTriangleArray = new int[widthInTiles * heightInTiles * MaxTCount];
            }

            public (int[] tileTriangleRecords, int[] tileTriangleCounts) GetCache()
            {
                return (perTileTriangleArray, perTileTriangleCountArray);
            }
        }
    }

}
