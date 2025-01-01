using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Renderer
{
    public struct Vertex
    {
        public Vector3 Position_ObjectSpace;
        public Vector3 Position_WorldSpace;
        public Vector3 Position_ScreenVolumeSpace;
        public Vector3 Normal_ObjectSpace;
        public Vector3 Normal_WorldSpace;
        public Vector2 UV;

        public Vector3 Tangent;     // 추가
        public Vector3 Bitangent;  // 추가
    }
    public class VertexShader
    {
        // GPU 커널: 행렬 변환과 벡터 변환을 수행
        public static void TransformVerticesKernel(
            Vertex[] vertices,
            float[] objectTransform)
        {
            Parallel.For(0, vertices.Length, (idx) =>
            {
                Vertex vertex = vertices[idx];

                Vector3 v = vertex.Position_ObjectSpace;
                // 오브젝트 변환 적용
                vertex.Position_WorldSpace = new Vector3(
                    v.x * objectTransform[0] + v.y * objectTransform[1] + v.z * objectTransform[2] + objectTransform[3],
                    v.x * objectTransform[4] + v.y * objectTransform[5] + v.z * objectTransform[6] + objectTransform[7],
                    v.x * objectTransform[8] + v.y * objectTransform[9] + v.z * objectTransform[10] + objectTransform[11])
                 / (v.x * objectTransform[12] + v.y * objectTransform[13] + v.z * objectTransform[14] + objectTransform[15]);

                v = vertex.Normal_ObjectSpace;
                vertex.Normal_WorldSpace = (new Vector3(
                    v.x * objectTransform[0] + v.y * objectTransform[1] + v.z * objectTransform[2] + objectTransform[3],
                    v.x * objectTransform[4] + v.y * objectTransform[5] + v.z * objectTransform[6] + objectTransform[7],
                    v.x * objectTransform[8] + v.y * objectTransform[9] + v.z * objectTransform[10] + objectTransform[11])
                 / (v.x * objectTransform[12] + v.y * objectTransform[13] + v.z * objectTransform[14] + objectTransform[15]));

                //v = vertex.Position_WorldSpace;
                //// 카메라 변환 적용
                //v = new Vector3(
                //    v.x * cameraTransform[0] + v.y * cameraTransform[1] + v.z * cameraTransform[2] + cameraTransform[3],
                //    v.x * cameraTransform[4] + v.y * cameraTransform[5] + v.z * cameraTransform[6] + cameraTransform[7],
                //    v.x * cameraTransform[8] + v.y * cameraTransform[9] + v.z * cameraTransform[10] + cameraTransform[11])
                // / (v.x * cameraTransform[12] + v.y * cameraTransform[13] + v.z * cameraTransform[14] + cameraTransform[15]);

                //vertex.Position_ScreenVolumeSpace = new Vector3(
                //    -(v.x) / v.z,
                //    -(v.y) / v.z,
                //(v.z));

                vertices[idx] = vertex;
            });
        }
        public static void TransformVerticesKernel2(
    Vertex[] vertices,
    float[] cameraTransform)
        {
            Parallel.For(0, vertices.Length, (idx) =>
            {
                Vertex vertex = vertices[idx];

                Vector3 v = vertex.Position_WorldSpace;
                // 카메라 변환 적용
                v = new Vector3(
                    v.x * cameraTransform[0] + v.y * cameraTransform[1] + v.z * cameraTransform[2] + cameraTransform[3],
                    v.x * cameraTransform[4] + v.y * cameraTransform[5] + v.z * cameraTransform[6] + cameraTransform[7],
                    v.x * cameraTransform[8] + v.y * cameraTransform[9] + v.z * cameraTransform[10] + cameraTransform[11])
                 / (v.x * cameraTransform[12] + v.y * cameraTransform[13] + v.z * cameraTransform[14] + cameraTransform[15]);
                vertex.Position_ScreenVolumeSpace = new Vector3(
    -(v.x) / v.z,
    -(v.y) / v.z,
(v.z));
                vertices[idx] = vertex;
            });
        }
        public Vertex[] Run_ObjectTransform(Vertex[] vertices, Matrix4x4 objectTransform)
        {
            // 변환 행렬을 1차원 배열로 변환
            float[] objTransformArray = Matrix4x4ToFloatArray(objectTransform);

            TransformVerticesKernel(vertices, objTransformArray);
            return vertices;
        }
        public Vertex[] Run_CameraTransform(Vertex[] vertices, Matrix4x4 cameraTransform)
        {
            // 변환 행렬을 1차원 배열로 변환
            float[] camTransformArray = Matrix4x4ToFloatArray(cameraTransform);

            TransformVerticesKernel2(vertices, camTransformArray);
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
        // Matrix4x4를 float 배열로 변환하는 함수
        private float[] Matrix4x4ToFloatArray(Matrix4x4 matrix)
        {
            return new float[]
            {
                matrix.e11, matrix.e12, matrix.e13, matrix.e14,
                matrix.e21, matrix.e22, matrix.e23, matrix.e24,
                matrix.e31, matrix.e32, matrix.e33, matrix.e34,
                matrix.e41, matrix.e42, matrix.e43, matrix.e44
            };
        }
    }
}
