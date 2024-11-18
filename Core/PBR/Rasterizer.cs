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
        const int MaxTCount = 64;

        private static Vector3 ConvertToScreenSpace(Vector3 position, int screenWidth, int screenHeight)
        {
            position.x = position.x * (screenWidth / 2.0f) + (screenWidth / 2.0f);
            position.y = position.y * (screenHeight / 2.0f) + (screenHeight / 2.0f);
            return position;
        }
        public static void ConvertVertexToScreenSpace(Vertex[] vertexes, int width, int height)
        {
            Parallel.For(0, vertexes.Length, (idx) =>
            {
                Vector3 v = vertexes[idx].Position_ScreenVolumeSpace;

                vertexes[idx].Position_ScreenVolumeSpace.x = vertexes[idx].Position_ScreenVolumeSpace.x * (width / 2.0f) + (width / 2.0f);
                vertexes[idx].Position_ScreenVolumeSpace.y = vertexes[idx].Position_ScreenVolumeSpace.y * (height / 2.0f) + (height / 2.0f);
            });
        }

        private TileCache tileCache;
        public Rasterizer(int vertexCount, int zBufferSize, int colorBufferSize, int triangleCount, int width, int height)
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


        private static Raster SetPixelWithZBuffer(int x, int y, FragmentShader shader, int triangleIndex, Vertex p1, Vertex p2, Vertex p3, float[] ZBuffer, int width)
        {
            int idx = x + y * width;
            
            Vector3 p = new Vector3(x, y, 0);
            float lambda1 = EdgeFunction(p2.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace, p);
            float lambda2 = EdgeFunction(p3.Position_ScreenVolumeSpace, p1.Position_ScreenVolumeSpace, p);
            float lambda3 = EdgeFunction(p1.Position_ScreenVolumeSpace, p2.Position_ScreenVolumeSpace, p);

            if(lambda1 > 0 && lambda2 >0 && lambda3 >0)
            {
                float areaABC = EdgeFunction(p1.Position_ScreenVolumeSpace, p2.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace);
                float zInterpolated = (lambda1 * p1.Position_ScreenVolumeSpace.z + lambda2 * p2.Position_ScreenVolumeSpace.z + lambda3 * p3.Position_ScreenVolumeSpace.z) / areaABC;
                if (zInterpolated < ZBuffer[idx])
                {
                    ZBuffer[idx] = zInterpolated;
                    p.z = zInterpolated;
                    Vector2 uv = (lambda1 * p1.UV + lambda2 * p2.UV + lambda3 * p3.UV) / (areaABC);

                    return new Raster(x, y, triangleIndex,
                        lambda1 * p1.Normal_WorldSpace + lambda2 * p2.Normal_WorldSpace + lambda3 * p3.Normal_WorldSpace,
                        new Vector3(lambda1, lambda2, lambda3),
                        p,
                        uv);
                }
            }
            return new Raster(x, y, -1, Vector3.zero, Vector3.zero, Vector3.zero, new Vector2(0, 0));
        }

        public static void CalculateCache_TrianglesOnPerTile(
            int[] triangles, Vertex[] vertexes, int[] perTileTriangleArray,
            int[] perTileTriangleCount,
            int screenWidth,
            int screenHeight)
        {
            
            Parallel.For(0, perTileTriangleCount.Length, (idx) =>
            {
                perTileTriangleCount[idx] = 0;
            });
            Parallel.For(0, triangles.Length/3, (idx) =>
            {
                Vertex p1 = vertexes[3 * idx];
                Vertex p2 = vertexes[3 * idx + 1];
                Vertex p3 = vertexes[3 * idx + 2];

                float e = EdgeFunction(p1.Position_ScreenVolumeSpace, p2.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace);
                if (e < 0)
                    return;

                float n_z = Vector3.Cross_Z(p2.Position_ScreenVolumeSpace - p1.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace - p1.Position_ScreenVolumeSpace);
                if (n_z > 0)
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
                        lock (perTileTriangleCount)
                        {
                            if (perTileTriangleCount[tileIndex] < MaxTCount)
                            {
                                int linearIndex = tileIndex * MaxTCount + perTileTriangleCount[tileIndex];
                                perTileTriangleArray[linearIndex] = idx;
                                perTileTriangleCount[tileIndex]++;
                            }
                        }
                    }
                }
            });
        }

        public static Raster[] TiledRasterizationKernel(
            FragmentShader shader,
            float[] zBuffer,
            int[] perTileTriangleArray,
            int[] perTileTriangleCount,
            Vertex[] vertexes,
            Raster[] rasters,
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
                int triangleCount = perTileTriangleCount[idx];

                for (int y = tileY; y < tileEndY; y++)
                {
                    for (int x = tileX; x < tileEndX; x++)
                    {
                        for (int i = 0; i < triangleCount; i++)
                        {
                            int record = perTileTriangleArray[idx * MaxTCount + i];

                            var r = SetPixelWithZBuffer(x, y, shader, record, vertexes[3 * record], vertexes[3 * record + 1], vertexes[3 * record + 2], zBuffer, screenWidth);
                            if (r.TriangleIndex == -1 && rasters[x + y * screenWidth].TriangleIndex != -1)
                            {
                                //유지
                            }
                            else
                                rasters[x + y * screenWidth] = r;
                        }
                    }
                }
            });
            return rasters;
        }
        readonly static FragmentShader Shader;
        Raster[] Rasters;
        public Raster[] RunTiled(Vertex[] vertices, FragmentShader f, float[] zBuffer, NBitmap target, int[] triangles, int width, int height)
        {
            var (tileTriangleRecords, tileTriangleCounts) = tileCache.GetCache();
            Parallel.For(0, Rasters.Length, (idx) =>
            {
                Rasters[idx].TriangleIndex = -1;
            });
            ConvertVertexToScreenSpace(vertices, width, height);
            CalculateCache_TrianglesOnPerTile(triangles, vertices, tileTriangleRecords, tileTriangleCounts, width, height);
            Rasters = TiledRasterizationKernel(f, zBuffer, tileTriangleRecords, tileTriangleCounts, vertices, Rasters, width, height);
            return Rasters;
        }
        public class TileCache
        {
            public int widthInTiles;
            public int heightInTiles;
            private int[] perTileTriangleArray; //value : triangleIndex
            private int[] perTileTriangleCountArray;

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
