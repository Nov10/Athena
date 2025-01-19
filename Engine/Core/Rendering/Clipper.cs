using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Athena.Maths;

namespace Athena.Engine.Core.Rendering
{
    public static class Clipper
    {
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
        static List<Vertex> ClipTriangleToFrustum(Vertex v1, Vertex v2, Vertex v3)
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

        public static (Vertex[], int[]) ClipTriangles_Parallel(Vertex[] vertices, int[] indices)
        {
            Vertex[] outputVertices = new Vertex[vertices.Length * 6]; // 클리핑 후 최대 크기 예상
            int[] outputIndices = new int[indices.Length * 6]; // 클리핑 후 최대 크기 예상
            int vertexCount = 0;
            int indexCount = 0;

            Parallel.For(0, indices.Length / 3, i =>
            {
                Vertex v1 = vertices[indices[i * 3 + 0]];
                Vertex v2 = vertices[indices[i * 3 + 1]];
                Vertex v3 = vertices[indices[i * 3 + 2]];

                Vertex[] clippedVertices = ClipTriangleToFrustum(v1, v2, v3, out int clippedCount);

                if (clippedCount >= 3)
                {
                    int localVertexIndex;
                    int localIndexIndex;

                    // Atomic increment for vertex count
                    localVertexIndex = Interlocked.Add(ref vertexCount, clippedCount) - clippedCount;

                    for (int j = 0; j < clippedCount; j++)
                    {
                        outputVertices[localVertexIndex + j] = clippedVertices[j];
                    }

                    // Atomic increment for index count
                    localIndexIndex = Interlocked.Add(ref indexCount, (clippedCount - 2) * 3) - (clippedCount - 2) * 3;

                    for (int j = 1; j < clippedCount - 1; j++)
                    {
                        outputIndices[localIndexIndex++] = localVertexIndex;
                        outputIndices[localIndexIndex++] = localVertexIndex + j;
                        outputIndices[localIndexIndex++] = localVertexIndex + j + 1;
                    }
                }
            });

            // 결과 배열 크기 조정
            Array.Resize(ref outputVertices, vertexCount);
            Array.Resize(ref outputIndices, indexCount);

            return (outputVertices, outputIndices);
        }

        static Vertex[] ClipTriangleToFrustum(Vertex v1, Vertex v2, Vertex v3, out int vertexCount)
        {
            Vertex[] verts = new Vertex[12]; // 클리핑 후 최대 크기 예상
            int count = 3;
            verts[0] = v1;
            verts[1] = v2;
            verts[2] = v3;

            for (int i = 0; i < 6; i++)
            {
                Vertex[] input = verts;
                int inputCount = count;
                verts = new Vertex[12];
                count = 0;
                Vector4 plane = GetClipPlane(i);

                for (int j = 0; j < inputCount; j++)
                {
                    Vertex curr = input[j];
                    Vertex nxt = input[(j + 1) % inputCount];

                    bool currInside = IsInsidePlane(curr.ClipPoint, plane);
                    bool nxtInside = IsInsidePlane(nxt.ClipPoint, plane);

                    if (currInside)
                    {
                        verts[count++] = curr;
                    }

                    if (currInside != nxtInside)
                    {
                        Vertex inter = IntersectPlane(plane, curr, nxt);
                        verts[count++] = inter;
                    }
                }
            }

            vertexCount = count;
            return verts;
        }
        static Vector4 GetClipPlane(int index)
        {
            switch (index)
            {
                case 0:
                    return new Vector4(1, 0, 0, 1);
                case 1:
                    return new Vector4(-1, 0, 0, 1);
                case 2:
                    return new Vector4(0, 1, 0, 1);
                case 3:
                    return new Vector4(0, -1, 0, 1);
                case 4:
                    return new Vector4(0, 0, 1, 1);
                case 5:
                    return new Vector4(0, 0, -1, 1);
            }
            return new Vector4(1, 0, 0, 1);
        }

        static bool IsInsidePlane(Vector4 point, Vector4 plane)
        {
            return point.x * plane.x + point.y * plane.y + point.z * plane.z + point.w * plane.w >= 0;
        }

        static Vertex IntersectPlane(Vector4 plane, Vertex v1, Vertex v2)
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
