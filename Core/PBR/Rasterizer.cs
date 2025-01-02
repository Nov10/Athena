using NPhotoshop.Core.Image;
using Renderer.Core.Shader;
using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public Vector3 Tangent;
        public Vector3 BitTangent;

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
        const int tileSize = 2;
        const int MaxTCount = 16;
        int[] TriangleIndices_PerTile; //value : triangleIndex
        int[] TriangleCount_PerTile; //value : count of triangles
        int Width;
        int Height;


    public Rasterizer(int width, int height)
        {
            int widthInTiles = width / tileSize;
            int heightInTiles = height / tileSize;
            int numTiles = widthInTiles * heightInTiles;
            Rasters = new Raster[width * height];

            Width = width;
            Height = height;

            TriangleCount_PerTile = new int[widthInTiles * heightInTiles];
            TriangleIndices_PerTile = new int[widthInTiles * heightInTiles * MaxTCount];
        }

        //Rasterizers
        /// <summary>
        /// Vertex의 ScreenVolumeSpace를 픽셀 단위의 화면 공간으로 변환
        /// </summary>
        public void ConvertVertexToScreenSpace(Vertex[] vertexes)
        {
            Parallel.For(0, vertexes.Length, (idx) =>
            {
                vertexes[idx].Position_ScreenVolumeSpace.x = vertexes[idx].Position_ScreenVolumeSpace.x * (Width / 2.0f) + (Width / 2.0f);
                vertexes[idx].Position_ScreenVolumeSpace.y = vertexes[idx].Position_ScreenVolumeSpace.y * (Height / 2.0f) + (Height / 2.0f);
            });
        }
        /// <summary>
        /// 화면을 구성하는 각 타일의 정보를 캐싱.
        /// (각 타일이 포함하는 삼각형의 개수와 인덱스를 캐싱)
        /// </summary>
        public void CalculateCache_TrianglesOnPerTile(int[] triangles, Vertex[] vertexes)
        {
            Parallel.For(0, triangles.Length/3, (idx) =>
            {
                Vertex p1 = vertexes[triangles[3 * idx]];
                Vertex p2 = vertexes[triangles[3 * idx + 1]];
                Vertex p3 = vertexes[triangles[3 * idx + 2]];

                if (EdgeFunction(p1.Position_ScreenVolumeSpace, p2.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace) < 0)
                    return;

                if (0 < Vector3.Cross_Z(p2.Position_ScreenVolumeSpace - p1.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace - p1.Position_ScreenVolumeSpace))
                    return;

                int tileStartX = Math.Max((int)Math.Floor(Math.Min(p1.Position_ScreenVolumeSpace.x, Math.Min(p2.Position_ScreenVolumeSpace.x, p3.Position_ScreenVolumeSpace.x)) / tileSize), 0);
                int tileStartY = Math.Max((int)Math.Floor(Math.Min(p1.Position_ScreenVolumeSpace.y, Math.Min(p2.Position_ScreenVolumeSpace.y, p3.Position_ScreenVolumeSpace.y)) / tileSize), 0);
                int tileEndX = Math.Min((int)Math.Ceiling(Math.Max(p1.Position_ScreenVolumeSpace.x, Math.Max(p2.Position_ScreenVolumeSpace.x, p3.Position_ScreenVolumeSpace.x)) / tileSize), Width / tileSize);
                int tileEndY = Math.Min((int)Math.Ceiling(Math.Max(p1.Position_ScreenVolumeSpace.y, Math.Max(p2.Position_ScreenVolumeSpace.y, p3.Position_ScreenVolumeSpace.y)) / tileSize), Height / tileSize);

                for (int tileY = tileStartY; tileY < tileEndY; tileY++)
                {
                    for (int tileX = tileStartX; tileX < tileEndX; tileX++)
                    {
                        int tileIndex = tileY * (Width / tileSize) + tileX;
                        
                        lock (TriangleCount_PerTile)
                        {
                            if (TriangleCount_PerTile[tileIndex] < MaxTCount)
                            {
                                int linearIndex = tileIndex * MaxTCount + TriangleCount_PerTile[tileIndex];
                                TriangleIndices_PerTile[linearIndex] = idx;
                                TriangleCount_PerTile[tileIndex]++;
                            }
                        }
                    }
                }
            });
        }
        /// <summary>
        /// 각 타일에 캐싱된 삼각형 데이터를 바탕으로 레스터를 갱신/계산
        /// </summary>
        public void CalculateRasters_PerTile(float[] zBuffer, Vertex[] vertexes, int[] triangles)
        {
            int numTiles = Width * Height / (tileSize * tileSize);
            Parallel.For(0, numTiles, (idx) =>
            {
                //타일의 시작 위치(왼쪽 아래)
                int tileX = (idx % (Width / tileSize)) * tileSize;
                int tileY = (idx / (Width / tileSize)) * tileSize;

                //타일의 끝 위치
                int tileEndX = Math.Min(tileX + tileSize, Width);
                int tileEndY = Math.Min(tileY + tileSize, Height);

                //타일에 포함된 삼각형의 개수
                int triangleCount = TriangleCount_PerTile[idx];

                for (int y = tileY; y < tileEndY; y++)
                {
                    for (int x = tileX; x < tileEndX; x++)
                    {
                        for (int i = 0; i < triangleCount; i++)
                        {
                            int record = TriangleIndices_PerTile[idx * MaxTCount + i];

                            SetRasterWithZBuffer(x, y, record, vertexes[triangles[3 * record]], vertexes[triangles[3 * record + 1]], vertexes[triangles[3 * record + 2]], zBuffer);
                        }
                    }
                }
            });
        }
        private void SetRasterWithZBuffer(int x, int y, int triangleIndex, Vertex p1, Vertex p2, Vertex p3, float[] ZBuffer)
        {
            int idx = x + y * Width;
            
            Vector3 p = new Vector3(x, y, 0);
            float lambda1 = EdgeFunction(p2.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace, p);
            if (lambda1 < 0)
                return;
            float lambda2 = EdgeFunction(p3.Position_ScreenVolumeSpace, p1.Position_ScreenVolumeSpace, p);
            if (lambda2 < 0)
                return;
            float lambda3 = EdgeFunction(p1.Position_ScreenVolumeSpace, p2.Position_ScreenVolumeSpace, p);
            if (lambda3 < 0)
                return;

            //if (lambda1 > 0 && lambda2 >0 && lambda3 >0)
            {
                //float areaABC = EdgeFunction(p1.Position_ScreenVolumeSpace, p2.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace);
                float areaABC = lambda1 + lambda2 + lambda3;
                float zInterpolated = (lambda1 * p1.Position_ScreenVolumeSpace.z + lambda2 * p2.Position_ScreenVolumeSpace.z + lambda3 * p3.Position_ScreenVolumeSpace.z) / areaABC;
                if (zInterpolated < ZBuffer[idx])
                {
                    //System.Diagnostics.Debug.WriteLine(new Vector3(xInterpolated, yInterpolated, zInterpolated));
                    if( -1 <= zInterpolated && zInterpolated <= 1)
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
                        Rasters[idx].Tangent = lambda1 * p1.Tangent + lambda2 * p2.Tangent + lambda3 * p3.Tangent;
                        Rasters[idx].BitTangent = lambda1 * p1.Bitangent + lambda2 * p2.Bitangent + lambda3 * p3.Bitangent;

                    }

                }
            }
        }

        private static float EdgeFunction(Vector3 a, Vector3 b, Vector3 c)
        {
            // 삼각형의 면적을 구하는 에지 함수
            //return -0.5f * (a.x * b.y + b.x * c.y + c.x * a.y - (b.x * a.y + c.x * b.y + a.x * c.y));
            return (c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x);
        }




        Raster[] Rasters;
        public Raster[] Run(Vertex[] vertices, float[] zBuffer, NBitmap target, int[] triangles, int width, int height)
        {
            Parallel.For(0, Rasters.Length, (idx) =>
            {
                Rasters[idx].TriangleIndex = -1;
            });
            Parallel.For(0, TriangleCount_PerTile.Length, (idx) =>
            {
                TriangleCount_PerTile[idx] = 0;
            });

            ConvertVertexToScreenSpace(vertices);
            CalculateCache_TrianglesOnPerTile(triangles, vertices);
            CalculateRasters_PerTile(zBuffer, vertices, triangles);
            return Rasters;
        }
    }

}
