using ILGPU.Runtime;
using ILGPU;
using Athena.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ILGPU.Runtime.Cuda;
using Athena.Engine.Core.Rendering.Shaders;

namespace Athena.Engine.Core.Rendering
{
    public class VertexShader
    {
        public Vertex[] Run(Vertex[] vertices, Vector3 objectPosition, CustomShader shader, Matrix4x4 M, Matrix4x4 VP, Matrix4x4 objectRotationTransform)
        {
            //TODO : VertexShader가 중간에 끼는데 어떻게 MVP를 그대로 씀? 함수를 따로 만들어야 하나...
            Parallel.For(0, vertices.Length, (idx) =>
            {
                Vertex vertex = vertices[idx];

                vertex.Position_WorldSpace = TransformMatrixCaculator.Transform(vertex.Position_ObjectSpace, M);
                vertex.Position_WorldSpace = shader.VertextShader(vertex.Position_WorldSpace, vertex.Normal_WorldSpace, objectPosition);
                vertex.ClipPoint = TransformMatrixCaculator.TransformH(vertex.Position_WorldSpace, VP);

                vertex.Normal_WorldSpace = TransformMatrixCaculator.Transform(vertex.Normal_ObjectSpace, objectRotationTransform);

                vertices[idx] = vertex;
            });
            return vertices;
        }
        public Vertex[] Run_ObjectTransform(Vertex[] vertices, Matrix4x4 objectTransform)
        {
            // 변환 행렬을 1차원 배열로 변환
            Parallel.For(0, vertices.Length, (idx) =>
            {
                Vertex vertex = vertices[idx];

                Vector3 v = vertex.Position_ObjectSpace;
                vertex.Position_WorldSpace = TransformMatrixCaculator.Transform(vertex.Position_ObjectSpace, objectTransform);
                vertex.Normal_WorldSpace = TransformMatrixCaculator.Transform(vertex.Normal_ObjectSpace, objectTransform);

                vertices[idx] = vertex;
            });
            return vertices;
        }
        public Vertex[] Run_CameraTransform(Vertex[] vertices, Matrix4x4 cameraTransform)
        {
            Parallel.For(0, vertices.Length, (idx) =>
            {
                Vertex vertex = vertices[idx];

                Vector3 v = vertex.Position_WorldSpace;
                v = TransformMatrixCaculator.Transform(v, cameraTransform);
                vertex.Position_ScreenVolumeSpace = new Vector3(
                -v.x,
                -v.y,
                 v.z);
                vertices[idx] = vertex;
            });
            return vertices;
        }
        public void Calc_T(Vertex[] vertices, int[] indices)
        {
            Parallel.For(0, indices.Length / 3, (idx) =>
            {
                Vertex v1 = vertices[indices[3 * idx]];
                Vertex v2 = vertices[indices[3 * idx + 1]];
                Vertex v3 = vertices[indices[3 * idx + 2]];

                // 위치 및 UV 좌표
                Vector3 p1 = v1.Position_ObjectSpace;
                Vector3 p2 = v2.Position_ObjectSpace;
                Vector3 p3 = v3.Position_ObjectSpace;

                Vector2 uv1 = v1.UV;
                Vector2 uv2 = v2.UV;
                Vector2 uv3 = v3.UV;

                // 삼각형의 Edge 벡터와 UV 변화량 계산
                Vector3 edge1 = p2 - p1;
                Vector3 edge2 = p3 - p1;

                Vector2 deltaUV1 = uv2 - uv1;
                Vector2 deltaUV2 = uv3 - uv1;

                float f = 1.0f / (deltaUV1.x * deltaUV2.y - deltaUV1.y * deltaUV2.x);

                // Tangent 벡터와 Bitangent 벡터 계산
                Vector3 tangent = f * (deltaUV2.y * edge1 - deltaUV1.y * edge2);
                Vector3 bitangent = f * (-deltaUV2.x * edge1 + deltaUV1.x * edge2);

                // 각 버텍스에 Tangent와 Bitangent를 누적
                v1.Tangent += tangent;
                v2.Tangent += tangent;
                v3.Tangent += tangent;

                v1.Bitangent += bitangent;
                v2.Bitangent += bitangent;
                v3.Bitangent += bitangent;
            });

            //// Tangent와 Bitangent를 정규화
            Parallel.For(0, vertices.Length, (idx) =>
            {
                vertices[idx].Tangent = vertices[idx].Tangent.normalized;
                vertices[idx].Bitangent = vertices[idx].Bitangent.normalized;
            });
        }
    }
}
