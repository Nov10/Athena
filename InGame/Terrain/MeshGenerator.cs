using Athena.Maths;
using Athena.Engine.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Engine.Core.Rendering.Shaders;

namespace Athena.InGame.Terrain
{
    public static class MeshGenerator
    {
        static float f(float x)
        {
            return (x - 2) * (x - 2) * (x - 2) * x;
        }
        static float Curve(float value)
        {
            return System.MathF.Max(0, f(2.1f * value + 0.9f) + 1.0f) / 4f;
        }
        public static TerrainMeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplider, int levelOfDetail)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            float halfWidth = (width - 1) / 2f;
            float halfHeight = (height - 1) / 2f;

            int meshSimplificationIncrement = levelOfDetail * 2;
            if (meshSimplificationIncrement <= 0)
                meshSimplificationIncrement = 1;

            int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

            TerrainMeshData meshData = new TerrainMeshData(verticesPerLine, verticesPerLine);
            int vertexIndex = 0;

            for (int y = 0; y < height; y += meshSimplificationIncrement)
            {
                for (int x = 0; x < width; x += meshSimplificationIncrement)
                {
                    meshData.Vertices[vertexIndex] = new Vector3(x - halfWidth, Curve(heightMap[x, y]) * heightMultiplider, y - halfHeight);
                    meshData.UVs[vertexIndex] = new Vector2((x / (float)width), (y / (float)height));

                    //오른쪽, 아래 가장자리 제외
                    if (x < width - 1 && y < height - 1)
                    {
                        meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                        meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                    }

                    vertexIndex++;
                }
            }

            return meshData;
        }
    }

    public class TerrainMeshData
    {
        public Vector3[] Vertices;
        public int[] Triangles;
        public Vector2[] UVs;

        int TriangleIndex;

        public TerrainMeshData(int width, int height)
        {
            Vertices = new Vector3[width * height];
            UVs = new Vector2[width * height];
            Triangles = new int[(width - 1) * (height - 1) * 6];
        }

        public void AddTriangle(int a, int b, int c)
        {
            Triangles[TriangleIndex] = a;
            Triangles[TriangleIndex + 1] = c;
            Triangles[TriangleIndex + 2] = b;
            TriangleIndex += 3;
        }

        public RenderData CreateMesh()
        {
            RenderData data = new RenderData();
            data.Vertices = new Vertex[Vertices.Length];
            Parallel.For(0, Vertices.Length, (idx) =>
            {
                Vertex v = new Vertex();
                v.UV = UVs[idx];
                v.Position_ObjectSpace = Vertices[idx];
                data.Vertices[idx] = v;

            });
            data.Triangles = Triangles;
            data.Shader = new NormalShader();
            data.CalculateNormals();
            data.CalculateAABB();

            return data;
        }
    }
}
