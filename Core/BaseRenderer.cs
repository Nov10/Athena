using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Renderer
{
    public class BaseRenderer
    {
        public int width;
        public int height;
        public Camera camera = new Camera
        {
            NearPlaneDistance = 3f,
            FarPlaneDistance = 100.0f,
            FieldOfView = 50f,
            AspectRatio = (float)1 / 1
        };

        public BaseRenderer(int w, int h)
        {
            width = w;
            height = h;
        }

        public void Run()
        {
            Camera camera = new Camera
            {
                FieldOfView = 1f,
                AspectRatio = (float)width / height,
                NearPlaneDistance = 0.1f,
                FarPlaneDistance = 100.0f
            };

            List<Vector3> cubeVertices = new List<Vector3>
            {
                new Vector3(2.0f, 2.0f, 2.0f),
                new Vector3(2.0f, 2.0f, -2.0f),
                new Vector3(2.0f, -2.0f, -2.0f),
                new Vector3(2.0f, -2.0f, 2.0f),
                new Vector3(-2.0f, 2.0f, 2.0f),
                new Vector3(-2.0f, 2.0f, -2.0f),
                new Vector3(-2.0f, -2.0f, -2.0f),
                new Vector3(-2.0f, -2.0f, 2.0f),
            };

            List<Tuple<int, int>> cubeEdges = new List<Tuple<int, int>>
            {
                Tuple.Create(0, 1), Tuple.Create(1, 2), Tuple.Create(2, 3), Tuple.Create(3, 0),
                Tuple.Create(4, 5), Tuple.Create(5, 6), Tuple.Create(6, 7), Tuple.Create(7, 4),
                Tuple.Create(0, 4), Tuple.Create(1, 5), Tuple.Create(2, 6), Tuple.Create(3, 7)
            };

            Vector3 objectPosition = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 objectRotation = new Vector3(0.0f, 0.0f, 0.0f);
            float time = 0;
            float speed = 1;
            float pi2 = (float)(2 * System.Math.PI);

            while (true)
            {
                time += 0.01f;
                time = time % pi2;

                // 카메라 행렬 계산
                Matrix4x4 viewMatrix = camera.CalculatePerspectiveMatrix();
                Matrix4x4 perspectiveMatrix = camera.CalculateProjectionMatrix();
                Matrix4x4 cameraMatrix = perspectiveMatrix * viewMatrix;

                // 물체의 위치와 회전 갱신
                objectPosition.x = (float)System.Math.Sin(speed * time);
                objectPosition.y = (float)System.Math.Cos(speed * time);
                objectPosition.z = (float)System.Math.Sin(speed * time);
                objectRotation.x = (float)System.Math.Sin(speed * time);
                objectRotation.y = (float)System.Math.Cos(speed * time);
                objectRotation.z = (float)System.Math.Sin(speed * time);

                Vector3[] transformedVertices = new Vector3[cubeVertices.Count];

                // 물체의 점들을 렌더링
                for (int i = 0; i < cubeVertices.Count; i++)
                {
                    Vector3 vertex = cubeVertices[i];

                    // 물체의 회전 행렬 생성 (Yaw-Pitch-Roll 순서)
                    Matrix4x4 rotationMatrix = CreateRotationMatrix(objectRotation);

                    // 물체의 위치 변환 행렬 생성
                    Matrix4x4 translationMatrix = CreateTranslationMatrix(objectPosition);

                    // 물체의 변환 행렬 (회전 후 이동)
                    Matrix4x4 objectTransform = rotationMatrix * translationMatrix;

                    // 정점 변환 (정점 -> 물체 변환 -> 카메라 변환)
                    Vector3 transformedVertex = Transform(vertex, objectTransform);
                    transformedVertex = Transform(transformedVertex, cameraMatrix);

                    // 직각 투영 변환
                    transformedVertices[i] = new Vector3(
                        transformedVertex.x / transformedVertex.z * width / 2 + width / 2,
                        -transformedVertex.y / transformedVertex.z * height / 2 + height / 2,
                        transformedVertex.z);

                    Console.WriteLine($"Vertex [{i}] : {transformedVertices[i]}");
                }

                // 물체의 선들을 렌더링
                foreach (var edge in cubeEdges)
                {
                    Vector3 pt1 = transformedVertices[edge.Item1];
                    Vector3 pt2 = transformedVertices[edge.Item2];

                    // 두 점을 연결하는 선을 그리는 로직
                    Console.WriteLine($"Drawing line from {pt1} to {pt2}");
                }

                // 여기서 실제로 이미지를 출력하거나 화면에 그리는 코드를 추가해야 함

                // 키 입력 처리 및 카메라 이동/회전 로직 추가 (생략)
            }
        }

        static Matrix4x4 CreateRotationMatrix(Vector3 rotation)
        {
            float cosX = (float)System.Math.Cos(rotation.x);
            float sinX = (float)System.Math.Sin(rotation.x);
            float cosY = (float)System.Math.Cos(rotation.y);
            float sinY = (float)System.Math.Sin(rotation.y);
            float cosZ = (float)System.Math.Cos(rotation.z);
            float sinZ = (float)System.Math.Sin(rotation.z);

            Matrix4x4 rotationX = new Matrix4x4(
                1, 0, 0, 0,
                0, cosX, -sinX, 0,
                0, sinX, cosX, 0,
                0, 0, 0, 1);

            Matrix4x4 rotationY = new Matrix4x4(
                cosY, 0, sinY, 0,
                0, 1, 0, 0,
                -sinY, 0, cosY, 0,
                0, 0, 0, 1);

            Matrix4x4 rotationZ = new Matrix4x4(
                cosZ, -sinZ, 0, 0,
                sinZ, cosZ, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);

            // Z * Y * X 순서로 회전 행렬을 곱합니다
            return rotationZ * rotationY * rotationX;
        }

        static Matrix4x4 CreateTranslationMatrix(Vector3 translation)
        {
            return new Matrix4x4(
                1, 0, 0, translation.x,
                0, 1, 0, translation.y,
                0, 0, 1, translation.z,
                0, 0, 0, 1);
        }

        static Vector3 Transform(Vector3 vector, Matrix4x4 matrix)
        {
            return new Vector3(
                vector.x * matrix.e11 + vector.y * matrix.e12 + vector.z * matrix.e13 + matrix.e14,
                vector.x * matrix.e21 + vector.y * matrix.e22 + vector.z * matrix.e23 + matrix.e24,
                vector.x * matrix.e31 + vector.y * matrix.e32 + vector.z * matrix.e33 + matrix.e34);
        }
    }
}
