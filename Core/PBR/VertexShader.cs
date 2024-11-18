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
    }
    public class VertexShader
    {
        // GPU 커널: 행렬 변환과 벡터 변환을 수행
        public static void TransformVerticesKernel(
            Vertex[] vertices,
            float[] objectTransform,
            float[] cameraTransform)
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

                v = vertex.Position_WorldSpace;
                // 카메라 변환 적용
                v = new Vector3(
                    v.x * cameraTransform[0] + v.y * cameraTransform[1] + v.z * cameraTransform[2] + cameraTransform[3],
                    v.x * cameraTransform[4] + v.y * cameraTransform[5] + v.z * cameraTransform[6] + cameraTransform[7],
                    v.x * cameraTransform[8] + v.y * cameraTransform[9] + v.z * cameraTransform[10] + cameraTransform[11])
                 / (v.x * cameraTransform[12] + v.y * cameraTransform[13] + v.z * cameraTransform[14] + cameraTransform[15]);

                //vertex.Position_ScreenVolumeSpace = new Vector3(
                //    -(v.x) / v.z,
                //    -(v.y) / v.z,
                //(v.z));

                vertices[idx] = vertex;
            });
        }
        public static void TransformVerticesKernel2(
    Vertex[] vertices,
    float[] objectTransform,
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
        public Vertex[] Run(Vertex[] vertices, Matrix4x4 objectTransform, Matrix4x4 cameraTransform)
        {
            // 변환 행렬을 1차원 배열로 변환
            float[] objTransformArray = Matrix4x4ToFloatArray(objectTransform);
            float[] camTransformArray = Matrix4x4ToFloatArray(cameraTransform);

            TransformVerticesKernel(vertices, objTransformArray, camTransformArray);
            return vertices;
        }
        public Vertex[] Run2(Vertex[] vertices, Matrix4x4 objectTransform, Matrix4x4 cameraTransform)
        {
            // 변환 행렬을 1차원 배열로 변환
            float[] objTransformArray = Matrix4x4ToFloatArray(objectTransform);
            float[] camTransformArray = Matrix4x4ToFloatArray(cameraTransform);

            TransformVerticesKernel2(vertices, objTransformArray, camTransformArray);
            return vertices;
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
