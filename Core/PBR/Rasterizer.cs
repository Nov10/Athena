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
                //ClipSpace -> NDC
                vertexes[idx].ClipPoint = vertexes[idx].ClipPoint / vertexes[idx].ClipPoint.w;
                //NDC -> Screen
                vertexes[idx].Position_ScreenVolumeSpace.x = -vertexes[idx].ClipPoint.x * (Width / 2.0f) + (Width / 2.0f);
                vertexes[idx].Position_ScreenVolumeSpace.y = -vertexes[idx].ClipPoint.y * (Height / 2.0f) + (Height / 2.0f);
                vertexes[idx].Position_ScreenVolumeSpace.z = vertexes[idx].ClipPoint.z;
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

                //if (0 < Vector3.Cross_Z(p2.Position_ScreenVolumeSpace - p1.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace - p1.Position_ScreenVolumeSpace))
                //    return;

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
                    if( -1 <= zInterpolated && zInterpolated <= 1f)
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



        #region A
        public (Vertex[], int[]) ClipTriangles(Vertex[] vertices, int[] indices)
        {
            List<Vertex> outputVertices = new List<Vertex>();
            List<int> outputIndices = new List<int>();

            for (int i = 0; i < indices.Length; i += 3)
            //Parallel.For(0, indices.Length / 3, (i) => { })
            {
                Vertex v1 = vertices[indices[i]];
                Vertex v2 = vertices[indices[i + 1]];
                Vertex v3 = vertices[indices[i + 2]];

                List<Vertex> clippedVertices = ClipTriangleToFrustum(v1, v2, v3);

                // 삼각형이 클리핑 후 여러 조각으로 나눠질 수 있음
                if (clippedVertices.Count >= 3)
                {
                    int baseIndex = outputVertices.Count;
                    outputVertices.AddRange(clippedVertices);

                    for (int j = 1; j < clippedVertices.Count - 1; j++)
                    {
                        outputIndices.Add(baseIndex);
                        outputIndices.Add(baseIndex + j);
                        outputIndices.Add(baseIndex + j + 1);
                    }
                }
            }

            return (outputVertices.ToArray(), outputIndices.ToArray());
        }

        private List<Vertex> ClipTriangleToFrustum(Vertex v1, Vertex v2, Vertex v3)
        {
            List<Vertex> vertices = new List<Vertex> { v1, v2, v3 };

            // 클리핑 평면 처리
            for (int i = 0; i < 6; i++) // 클리핑 평면은 x, y, z에 대해 +/- 6개
            {
                List<Vertex> inputVertices = vertices;
                vertices = new List<Vertex>();

                Vector4 plane = GetClipPlane(i);

                for (int j = 0; j < inputVertices.Count; j++)
                {
                    Vertex current = inputVertices[j];
                    Vertex next = inputVertices[(j + 1) % inputVertices.Count];

                    bool currentInside = IsInsidePlane(current.ClipPoint, plane);
                    bool nextInside = IsInsidePlane(next.ClipPoint, plane);

                    if (currentInside)
                        vertices.Add(current);

                    if (currentInside != nextInside)
                    {
                        Vertex intersection = IntersectPlane(plane, current, next);
                        vertices.Add(intersection);
                    }
                }
            }

            return vertices;
        }

        private Vector4 GetClipPlane(int index)
        {
            // 클리핑 평면 정의 (x, y, z, w 각각 +/- 방향)
            return index switch
            {
                0 => new Vector4(1, 0, 0, 1), // x = w
                1 => new Vector4(-1, 0, 0, 1), // x = -w
                2 => new Vector4(0, 1, 0, 1), // y = w
                3 => new Vector4(0, -1, 0, 1), // y = -w
                4 => new Vector4(0, 0, 1, 1), // z = w
                5 => new Vector4(0, 0, -1, 1), // z = -w
                _ => throw new ArgumentException("Invalid clip plane index")
            };
        }

        private bool IsInsidePlane(Vector4 point, Vector4 plane)
        {
            return point.x * plane.x + point.y * plane.y + point.z * plane.z + point.w * plane.w >= 0;
        }

        private Vertex IntersectPlane(Vector4 plane, Vertex v1, Vertex v2)
        {
            float t = -(v1.ClipPoint.x * plane.x + v1.ClipPoint.y * plane.y + v1.ClipPoint.z * plane.z + v1.ClipPoint.w * plane.w) /
                      ((v2.ClipPoint.x - v1.ClipPoint.x) * plane.x +
                       (v2.ClipPoint.y - v1.ClipPoint.y) * plane.y +
                       (v2.ClipPoint.z - v1.ClipPoint.z) * plane.z +
                       (v2.ClipPoint.w - v1.ClipPoint.w) * plane.w);

            Vertex result = v1;
            result.ClipPoint = v1.ClipPoint + t * (v2.ClipPoint - v1.ClipPoint);
            result.Position_WorldSpace = v1.Position_WorldSpace + t * (v2.Position_WorldSpace - v1.Position_WorldSpace);
            result.UV = v1.UV + t * (v2.UV - v1.UV);
            result.Normal_WorldSpace = v1.Normal_WorldSpace + t * (v2.Normal_WorldSpace - v1.Normal_WorldSpace);
            return result;
        }
        #endregion
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
            (vertices, triangles) = ClipTriangles(vertices, triangles);
            ConvertVertexToScreenSpace(vertices);
            CalculateCache_TrianglesOnPerTile(triangles, vertices);
            CalculateRasters_PerTile(zBuffer, vertices, triangles);
            return Rasters;
        }
    }

}
