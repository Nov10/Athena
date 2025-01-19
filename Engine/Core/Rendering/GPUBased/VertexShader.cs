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
        private Action<Index1D, ArrayView<Vertex>, Matrix4x4, Matrix4x4, Matrix4x4> Kernel_ConvertObjectSpace2WorldSpace;
        private Action<Index1D, ArrayView<Vertex>, Matrix4x4, Matrix4x4, Matrix4x4> Kernel_ConvertWorldSpace2ClipSpace;

        public VertexShader()
        {
            Kernel_ConvertObjectSpace2WorldSpace = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<Vertex>, Matrix4x4, Matrix4x4, Matrix4x4>
                (ConvertObjectSpace2WorldSpace);
            Kernel_ConvertWorldSpace2ClipSpace = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<Vertex>, Matrix4x4, Matrix4x4, Matrix4x4>
                (ConvertConvertWorldSpace2ClipSpace);
        }
        public static void ConvertObjectSpace2WorldSpace(Index1D idx, ArrayView<Vertex> vertices, Matrix4x4 m, Matrix4x4 vp, Matrix4x4 obj)
        {
            Vertex vertex = vertices[idx];
            vertex.Position_WorldSpace = TransformMatrixCaculator.Transform(vertex.Position_ObjectSpace, m);
            vertex.Normal_WorldSpace = TransformMatrixCaculator.Transform(vertex.Normal_ObjectSpace, obj);

            vertices[idx] = vertex;
        }
        public static void ConvertConvertWorldSpace2ClipSpace(Index1D idx, ArrayView<Vertex> vertices, Matrix4x4 m, Matrix4x4 vp, Matrix4x4 obj)
        {
            vertices[idx].ClipPoint = TransformMatrixCaculator.TransformH(vertices[idx].Position_WorldSpace, vp);
        }
        public void Run(MemoryBuffer1D<Vertex, Stride1D.Dense>  vertices,  Vector3 objectPosition, CustomShader shader, Matrix4x4 M, Matrix4x4 VP, Matrix4x4 objectRotationTransform, int length)
        {
            Kernel_ConvertObjectSpace2WorldSpace((int)length, vertices.View, M, VP, objectRotationTransform);
            shader.RunVertexShader_GPU(vertices, objectPosition, length);
            Kernel_ConvertWorldSpace2ClipSpace((int)length, vertices.View, M, VP, objectRotationTransform);
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
