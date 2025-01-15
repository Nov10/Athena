using System;
using System.Collections.Generic;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using Athena.Maths;
using ILGPU.Algorithms;
using System.Threading.Tasks;
using Athena.Engine.Core.Rendering;

/* 병렬로 레스터라이저를 구현할 때, 가장 먼저 고려해야 할 것은 '반복문을 삼각형을 기준으로 할지, 픽셀을 기준으로 할지'이다.
 * 반복문을 삼각형을 기준으로 돌린다면...
 * 각 삼각형은 모든 픽셀에 대해 그 삼각형이 보이는지를 확인해야만 한다. FHD의 해상도만 봐도 꽤나 비싼 작업이다.
 * 반복문을 픽셀을 기준으로 돌린다면...
 * 각 픽셀은 모든 삼각형에 대해 그 픽셀에 삼각형이 보이는지를 확인해야만 한다. 그러나 삼각형의 개수가 어느정도일지 모르기 때문에, 이 또한 비싼 작업이 될 수 있다.
 * 
 * 따라서 삼각형과 픽셀 사이에 타일을 만들어 구현한다.
 * 타일은 '화면을 구성하는 작은 판(픽셀 모음)'이며, 2x2, 4x4, 8x8 등 꽤나 작은 크기로 구성된다.
 * 그리고 반복문을 삼각형에 대해서 한번, 타일에 대해서 한번 돌린다.
 * 
 * 각 삼각형에 대해...
 * 각 삼각형은 그 삼각형이 어느 '타일'에 보이는지를 계산한다. 타일의 수는 픽셀의 수보다 현저히 작으므로, 단순 픽셀에 대한 반복과 비교하면 꽤나 유리하다.
 * 만일 하나의 삼각형이 특정 타일에 보인다면, 그 타일에 삼각형의 인덱스를 기록한다. 즉, 캐싱한다.
 * 다음으로,
 * 각 타일에 대해...
 * 각 타일은 삼각형에 대해 캐싱된 정보를 지니고 있다. 즉, 각 타일은 그 타일에 어느 삼각형이 보일지를 알고 있다.
 * 각 타일은 그 타일을 구성하는 모든 픽셀들에 대한 작은 반복문을 돌려, 그 픽셀에서 캐싱된 삼각형들 중 어느 것이 보일지 계산한다.
 * 이때 삼각형의 최대 개수는 정해져 있으므로(정할 수밖에 없다. 아니면 동적으로 크기를 정해야 하는데, 이건 좀 그렇다), 너무 오랜 시간 연산이 수행되는 것을 방지할 수 있다.
 * 
 * 정리하자면,
 * 삼각형이 어느 타일에 보이는지 계산 -> 타일에서 그 삼각형이 타일을 구성하는 픽셀 중 어디에 보이는지 계산
 * 확실하진 않지만, 현대 GPU는 이렇게 구현된다고 한다. */

namespace Athena.Engine.Core.Rendering
{
    /// <summary>
    /// GPU-Based Rasterizer. **이 클래스는 라이브러리에 의존합니다.**
    /// </summary>
    public partial class GPURasterizer/*.Kernels*/
    {
        /// <summary>
        /// 정점을 Clip Space -> Screen Space로 변환
        /// </summary>
        public static void InternelKernel_ConvertVertexToScreenSpace(Index1D idx, ArrayView<Vertex> vertexes, int width, int height)
        {
            if (idx >= vertexes.Length) return;

            // ClipSpace -> NDC
            Vector4 clipPos = vertexes[idx].ClipPoint / vertexes[idx].ClipPoint.w;

            // NDC -> Screen
            vertexes[idx].Position_ScreenVolumeSpace.x = (-clipPos.x + 1) * (width * 0.5f);
            vertexes[idx].Position_ScreenVolumeSpace.y = (-clipPos.y + 1) * (height * 0.5f);
            vertexes[idx].Position_ScreenVolumeSpace.z = clipPos.z;
        }


        /// <summary>
        /// 타일에 삼각형을 캐싱
        /// </summary>
        /// <param name="triangleIdx"></param>
        /// <param name="triangleIndicesPerTile">각 타일별 삼각형 인덱스 배열</param>
        /// <param name="triangleCountPerTile">각 타일에 보여지는 삼각형 개수의 배열</param>
        public static void InternalKernel_CalculateCacheTrianglesPerTile(
            Index1D triangleIdx,                  // 삼각형 인덱스 (0 ~ triangles.Length/3-1)
            ArrayView<int> triangleIndicesPerTile,// tile마다 기록하는 삼각형 인덱스
            ArrayView<int> triangleCountPerTile,  // tile마다 삼각형 개수 카운트
            ArrayView<Vertex> vertices,
            ArrayView<int> triangles,
            int width,
            int height,
            int tileSize,
            int MaxTCount)
        {
            Vertex p1 = vertices[triangles[triangleIdx * 3 + 0]];
            Vertex p2 = vertices[triangles[triangleIdx * 3 + 1]];
            Vertex p3 = vertices[triangles[triangleIdx * 3 + 2]];

            if (0 < Vector3.Cross_Z(p2.Position_ScreenVolumeSpace - p1.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace - p1.Position_ScreenVolumeSpace))
            {
                return;
            }

            // 타일 범위 구하기
            int tileStartX = (int)IntrinsicMath.Max(
                0,
                ILGPU.Algorithms.XMath.Floor(
                    IntrinsicMath.Min(p1.Position_ScreenVolumeSpace.x,
                              IntrinsicMath.Min(p2.Position_ScreenVolumeSpace.x, p3.Position_ScreenVolumeSpace.x)) / tileSize));
            int tileStartY = (int)IntrinsicMath.Max(
                0,
                ILGPU.Algorithms.XMath.Floor(
                    IntrinsicMath.Min(p1.Position_ScreenVolumeSpace.y,
                              IntrinsicMath.Min(p2.Position_ScreenVolumeSpace.y, p3.Position_ScreenVolumeSpace.y)) / tileSize));
            int tileEndX = (int)IntrinsicMath.Min(
                width / tileSize,
                ILGPU.Algorithms.XMath.Ceiling(
                    IntrinsicMath.Max(p1.Position_ScreenVolumeSpace.x,
                              IntrinsicMath.Max(p2.Position_ScreenVolumeSpace.x, p3.Position_ScreenVolumeSpace.x)) / tileSize));
            int tileEndY = (int)IntrinsicMath.Min(
                height / tileSize,
                ILGPU.Algorithms.XMath.Ceiling(
                    IntrinsicMath.Max(p1.Position_ScreenVolumeSpace.y,
                              IntrinsicMath.Max(p2.Position_ScreenVolumeSpace.y, p3.Position_ScreenVolumeSpace.y)) / tileSize));

            // 타일들을 순회
            for (int ty = tileStartY; ty < tileEndY; ty++)
            {
                for (int tx = tileStartX; tx < tileEndX; tx++)
                {
                    int tileIndex = ty * (width / tileSize) + tx;

                    // oldVal = triangleCountPerTile[tileIndex]++
                    int oldVal = Atomic.Add(ref triangleCountPerTile[tileIndex], 1);
                    if (oldVal < MaxTCount)
                    {
                        triangleIndicesPerTile[tileIndex * MaxTCount + oldVal] = triangleIdx;
                    }
                }
            }
        }


        /// <summary>
        /// 각 레스터에 대해 삼각형을 계산
        /// </summary>
        /// <param name="triangleIndicesPerTile">각 타일별 삼각형 인덱스 배열</param>
        /// <param name="triangleCountPerTile">각 타일에 보여지는 삼각형 개수의 배열</param>
        public static void InternalKernel_CalculateRastersPerTile(
            Index1D idx,                    // tile index
            ArrayView<float> zBuffer,
            ArrayView<Vertex> vertices,
            ArrayView<int> triangles,
            ArrayView<int> triangleIndicesPerTile,
            ArrayView<int> triangleCountPerTile,
            ArrayView<Raster> rasters,
            int width,
            int height,
            int tileSize,
            int MaxTCount)
        {

            int tilesX = width / tileSize;   // 타일의 가로 개수
                                             // idx -> tileX, tileY 계산
            int tileY = idx / tilesX;
            int tileX = idx % tilesX;

            // 실제 픽셀 범위
            int startX = tileX * tileSize;
            int startY = tileY * tileSize;
            int endX = ILGPU.Algorithms.XMath.Min(startX + tileSize, width);
            int endY = ILGPU.Algorithms.XMath.Min(startY + tileSize, height);

            int triangleCount = triangleCountPerTile[idx];

            // 각 픽셀 순회
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    // 타일에 속한 삼각형들
                    for (int i = 0; i < triangleCount; i++)
                    {
                        if (idx * MaxTCount + i >= triangleIndicesPerTile.Length || idx < 0 || idx >= triangleCountPerTile.Length)
                            return;
                        int record = triangleIndicesPerTile[idx * MaxTCount + i];
                        if (record < 0 || record * 3 + 2 >= triangles.Length)
                            return;
                        SetRasterWithZBuffer(
                            x, y,
                            record,  // triangleIndex
                            vertices[triangles[record * 3 + 0]], vertices[triangles[record * 3 + 1]], vertices[triangles[record * 3 + 2]],
                            zBuffer,
                            rasters,
                            width,
                            height
                        );
                    }
                }
            }
        }

        /// <summary>
        /// SetRasterWithZBuffer 로직을 GPU에서 호출할 수 있도록 static 함수화
        /// (Rasters 배열에 직접 쓰려면, Rasters도 ArrayView<Raster>로 넘겨줘야 함.)
        /// </summary>
        public static void SetRasterWithZBuffer(
            int x, int y,
            int triangleIndex,
            Vertex p1, Vertex p2, Vertex p3,
            ArrayView<float> ZBuffer,
            ArrayView<Raster> Rasters,
            int width,
            int height)
        {
            int idx = x + y * width;
            // EdgeFunction
            Vector3 P = new Vector3(x, y, 0);

            float lambda1 = EdgeFunction(p2.Position_ScreenVolumeSpace, p3.Position_ScreenVolumeSpace, P);
            if (lambda1 < 0) return;

            float lambda2 = EdgeFunction(p3.Position_ScreenVolumeSpace, p1.Position_ScreenVolumeSpace, P);
            if (lambda2 < 0) return;

            float lambda3 = EdgeFunction(p1.Position_ScreenVolumeSpace, p2.Position_ScreenVolumeSpace, P);
            if (lambda3 < 0) return;

            float areaABC = 1.0f / (lambda1 + lambda2 + lambda3);
            lambda1 *= areaABC;
            lambda2 *= areaABC;
            lambda3 *= areaABC;

            float zInterpolated = lambda1 * p1.Position_ScreenVolumeSpace.z +
                                  lambda2 * p2.Position_ScreenVolumeSpace.z +
                                  lambda3 * p3.Position_ScreenVolumeSpace.z;

            //경합 발생 안함
            float oldZ = ZBuffer[idx];
            if (zInterpolated < ZBuffer[idx] && -1 <= zInterpolated && zInterpolated <= 1f)
            {
                ZBuffer[idx] = zInterpolated;
                Rasters[idx].x = x;
                Rasters[idx].y = y;
                Rasters[idx].TriangleIndex = triangleIndex;
                Rasters[idx].Normal_WorldSpace = (lambda1 * p1.Normal_WorldSpace + lambda2 * p2.Normal_WorldSpace + lambda3 * p3.Normal_WorldSpace);
                //Rasters[idx].Tangent = lambda1 * p1.Tangent + lambda2 * p2.Tangent + lambda3 * p3.Tangent;
                //Rasters[idx].BitTangent = lambda1 * p1.Bitangent + lambda2 * p2.Bitangent + lambda3 * p3.Bitangent;

                //UV 보정
                float z = 1.0f / zInterpolated;
                float invW1 = 1 / p1.ClipPoint.w;
                float invW2 = 1 / p2.ClipPoint.w;
                float invW3 = 1 / p3.ClipPoint.w;
                lambda1 = lambda1 * invW1 * z;
                lambda2 = lambda2 * invW2 * z;
                lambda3 = lambda3 * invW3 * z;
                areaABC = lambda1 + lambda2 + lambda3;
                Rasters[idx].UV = (lambda1 * p1.UV + lambda2 * p2.UV + lambda3 * p3.UV) / areaABC;
            }
        }

        public static void ClearZBufferKernel(Index1D idx, ArrayView<float> zBuffer)
        {
            zBuffer[idx] = float.MaxValue;
        }
        public static void ClearRastersKernel(Index1D idx, ArrayView<Raster> rasters)
        {
            rasters[idx].TriangleIndex = -1;
        }
        public static void ClearTriangleCacheKernel(Index1D idx, ArrayView<int> triangles)
        {
            triangles[idx] = 0;
        }

        public static float EdgeFunction(Vector3 a, Vector3 b, Vector3 c)
        {
            return (c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x);
        }


        public static (Vertex[], int[]) ClipTriangles(Vertex[] vertices, int[] indices)
        {
            List<Vertex> outputVertices = new List<Vertex>();
            List<int> outputIndices = new List<int>();

            for (int i = 0; i < indices.Length; i += 3)
            {
                Vertex v1 = vertices[indices[i + 0]];
                Vertex v2 = vertices[indices[i + 1]];
                Vertex v3 = vertices[indices[i + 2]];

                List<Vertex> clippedVertices = ClipTriangleToFrustum(v1, v2, v3);

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

        public static List<Vertex> ClipTriangleToFrustum(Vertex v1, Vertex v2, Vertex v3)
        {
            List<Vertex> verts = new List<Vertex> { v1, v2, v3 };
            for (int i = 0; i < 6; i++)
            {
                List<Vertex> input = verts;
                verts = new List<Vertex>();
                Vector4 plane = GetClipPlane(i);

                for (int j = 0; j < input.Count; j++)
                {
                    Vertex curr = input[j];
                    Vertex nxt = input[(j + 1) % input.Count];

                    bool currInside = IsInsidePlane(curr.ClipPoint, plane);
                    bool nxtInside = IsInsidePlane(nxt.ClipPoint, plane);

                    if (currInside)
                        verts.Add(curr);

                    if (currInside != nxtInside)
                    {
                        Vertex inter = IntersectPlane(plane, curr, nxt);
                        verts.Add(inter);
                    }
                }
            }
            return verts;
        }

        public static Vector4 GetClipPlane(int index)
        {
            return index switch
            {
                0 => new Vector4(1, 0, 0, 1),
                1 => new Vector4(-1, 0, 0, 1),
                2 => new Vector4(0, 1, 0, 1),
                3 => new Vector4(0, -1, 0, 1),
                4 => new Vector4(0, 0, 1, 1),
                5 => new Vector4(0, 0, -1, 1),
                _ => throw new ArgumentException("Invalid clip plane index")
            };
        }

        public static bool IsInsidePlane(Vector4 point, Vector4 plane)
        {
            return point.x * plane.x + point.y * plane.y + point.z * plane.z + point.w * plane.w >= 0;
        }

        public static Vertex IntersectPlane(Vector4 plane, Vertex v1, Vertex v2)
        {
            float t = -(v1.ClipPoint.x * plane.x
                      + v1.ClipPoint.y * plane.y
                      + v1.ClipPoint.z * plane.z
                      + v1.ClipPoint.w * plane.w)
                    / ((v2.ClipPoint.x - v1.ClipPoint.x) * plane.x
                      + (v2.ClipPoint.y - v1.ClipPoint.y) * plane.y
                      + (v2.ClipPoint.z - v1.ClipPoint.z) * plane.z
                      + (v2.ClipPoint.w - v1.ClipPoint.w) * plane.w);

            Vertex res = v1;
            res.ClipPoint = v1.ClipPoint + t * (v2.ClipPoint - v1.ClipPoint);
            res.Position_WorldSpace = v1.Position_WorldSpace + t * (v2.Position_WorldSpace - v1.Position_WorldSpace);
            res.Normal_WorldSpace = v1.Normal_WorldSpace + t * (v2.Normal_WorldSpace - v1.Normal_WorldSpace);
            res.UV = v1.UV + t * (v2.UV - v1.UV);
            return res;
        }
    }
}
