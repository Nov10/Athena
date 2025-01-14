using Athena.Maths;
using Athena.Engine.Core.Rendering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Athena.Engine.Core.Image;

namespace Athena.Engine.Core.Rendering
{
    public class RenderData
    {
        public Vertex[] Vertices;
        public int[] Triangles;
        public Color[] Colors;

        public CustomShader Shader;

        public AABB ThisAABB;

        public void CalculateNormals()
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal_ObjectSpace = new Vector3();
            }
            for (int i = 0; i < Triangles.Length / 3; i++)
            {
                var p1 = Vertices[Triangles[3 * i]];
                var p2 = Vertices[Triangles[3 * i + 1]];
                var p3 = Vertices[Triangles[3 * i + 2]];

                Vector3 normal = Vector3.Cross(p2.Position_ObjectSpace - p1.Position_ObjectSpace, p3.Position_ObjectSpace - p1.Position_ObjectSpace);
                p1.Normal_ObjectSpace += normal;
                p2.Normal_ObjectSpace += normal;
                p3.Normal_ObjectSpace += normal;

                Vertices[Triangles[3 * i]] = p1;
                Vertices[Triangles[3 * i + 1]] = p2;
                Vertices[Triangles[3 * i + 2]] = p3;
            }
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal_ObjectSpace = Vertices[i].Normal_ObjectSpace.normalized;
            }
        }

        public void CalculateAABB()
        {
            ThisAABB = new AABB(Vertices);
        }

        public RenderData(RenderData data)
        {
            //this.Vertices = (Vector3[]) data.Vertices.Clone();
            Vertices = (Vertex[])data.Vertices.Clone();
            Triangles = (int[])data.Triangles.Clone();
            Colors = data.Colors;
            Shader = data.Shader;
            ThisAABB = data.ThisAABB;
        }
        public RenderData()
        {

        }
        public static RenderData CreateCube1x1()
        {
            // 새 RenderData 객체 생성
            RenderData renderData = new RenderData();

            // --------
            // 1) 정점(Vertices) 정의
            // --------
            //  8개의 코너(-0.5 ~ +0.5 범위)
            //  
            //    (-0.5, 0.5, -0.5)    ( 0.5, 0.5, -0.5)
            //          3 ---------------- 2
            //          |                  |
            //          |                  |
            //          |                  |
            //          0 ---------------- 1
            //    (-0.5, -0.5, -0.5)   ( 0.5, -0.5, -0.5)
            //
            //    (-0.5, 0.5,  0.5)    ( 0.5, 0.5,  0.5)
            //          7 ---------------- 6
            //          |                  |
            //          |                  |
            //          |                  |
            //          4 ---------------- 5
            //    (-0.5, -0.5,  0.5)   ( 0.5, -0.5,  0.5)
            //
            // 정점은 총 8개이며, 각 Vertex에는 위치(Position_ObjectSpace), 노말, UV 등을 넣을 수 있습니다.
            // 여기서는 위치만 일단 지정하고, 나중에 CalculateNormals()로 노말을 계산합니다.

            renderData.Vertices = new Vertex[]
            {
            // 0
            new Vertex { Position_ObjectSpace = new Vector3(-0.5f, -0.5f, -0.5f) },
            // 1
            new Vertex { Position_ObjectSpace = new Vector3( 0.5f, -0.5f, -0.5f) },
            // 2
            new Vertex { Position_ObjectSpace = new Vector3( 0.5f,  0.5f, -0.5f) },
            // 3
            new Vertex { Position_ObjectSpace = new Vector3(-0.5f,  0.5f, -0.5f) },

            // 4
            new Vertex { Position_ObjectSpace = new Vector3(-0.5f, -0.5f,  0.5f) },
            // 5
            new Vertex { Position_ObjectSpace = new Vector3( 0.5f, -0.5f,  0.5f) },
            // 6
            new Vertex { Position_ObjectSpace = new Vector3( 0.5f,  0.5f,  0.5f) },
            // 7
            new Vertex { Position_ObjectSpace = new Vector3(-0.5f,  0.5f,  0.5f) }
            };

            // --------
            // 2) 삼각형(Triangles) 정의
            // --------
            // 큐브는 6개의 면 * 각 면당 2개의 삼각형 = 총 12개의 삼각형, 인덱스로는 36개.
            // 아래는 각 면을 시계/반시계 방향으로 이어 붙인 예시입니다.
            renderData.Triangles = new int[]
            {
            // Front(정면):  z = +0.5
            4, 5, 6,
            4, 6, 7,

            // Right(오른쪽): x = +0.5
            5, 1, 2,
            5, 2, 6,

            // Back(후면): z = -0.5
            1, 0, 3,
            1, 3, 2,

            // Left(왼쪽): x = -0.5
            0, 4, 7,
            0, 7, 3,

            // Top(위): y = +0.5
            3, 7, 6,
            3, 6, 2,

            // Bottom(아래): y = -0.5
            1, 5, 4,
            1, 4, 0
            };

            // --------
            // 3) 색상(Colors) 지정 (필요 시)
            // --------
            // 꼭 필요한 것은 아니지만, 원하는 단색으로 초기화할 수도 있습니다.
            // 여기서는 일괄적으로 White 컬러를 적용합니다.
            renderData.Colors = new Color[renderData.Vertices.Length];
            for (int i = 0; i < renderData.Colors.Length; i++)
            {
                renderData.Colors[i] = Color.white;
            }

            // --------
            // 4) 노말 계산 및 AABB 계산
            // --------
            // 작성해 주신 RenderData 코드의 CalculateNormals()를 통해 노말 벡터를 계산합니다.
            renderData.CalculateNormals();

            // AABB(바운딩 박스)도 자동으로 계산
            renderData.CalculateAABB();

            // 생성이 끝난 1x1x1 큐브 RenderData 반환
            return renderData;
        }

        private static void AddSegment(
            ref Vertex[] vertices,
            ref int[] triangles,
            Vector3 centerPos,
            Quaternion rotation,
            float length,
            float width,
            float height,
            Color color
        )
        {
            // 1) 기본 1×1×1 박스의 local 정점 & 인덱스 생성
            //    (아래는 예시 함수이며, 꼭 이 형태가 아니어도 됩니다)
            Vertex[] boxVertices = CreateUnitBox(out int[] boxTriangles);

            // 2) 박스의 local scale을 (width, height, length)로 만든다.
            //    - (1×1×1) 기준에서 X->width, Y->height, Z->length
            Vector3 scale = new Vector3(width, height, length);

            // 3) 모든 정점을 변환해서 최종 월드 좌표로 만든다.
            //    - 로컬 포지션 → Scale → 회전(Quaternion) → 월드 중심(centerPos) 이동
            //    - 만약 Athena.Maths에 Matrix4x4가 있다면, 직접 행렬 곱으로 해도 되고,
            //      간단히 scale + quaternion + translation을 순서대로 적용해도 됩니다.
            for (int i = 0; i < boxVertices.Length; i++)
            {
                // (1) 스케일
                Vector3 localPos = boxVertices[i].Position_ObjectSpace;
                localPos.x *= scale.x;
                localPos.y *= scale.y;
                localPos.z *= scale.z;

                // (2) 회전
                localPos = rotation.RotateVector( localPos);

                // (3) 최종 월드 위치로 이동
                localPos += centerPos;

                // 위치 적용
                boxVertices[i].Position_ObjectSpace = localPos;

                // 색상(옵션)
                //boxVertices[i].col = color;
            }

            // 4) 현재까지 쌓인 vertices 개수를 파악한 뒤, index offset을 적용해 넣어준다.
            int baseIndex = vertices.Length;  // Merge 직전에 이미 들어있던 정점 수

            // 5) vertices 배열에 boxVertices를 이어붙이기
            Array.Resize(ref vertices, baseIndex + boxVertices.Length);
            for (int i = 0; i < boxVertices.Length; i++)
            {
                vertices[baseIndex + i] = boxVertices[i];
            }

            // 6) triangles도 마찬가지로 offset을 적용해서 이어붙이기
            int triBase = triangles.Length;
            Array.Resize(ref triangles, triBase + boxTriangles.Length);
            for (int i = 0; i < boxTriangles.Length; i++)
            {
                // boxTriangles[i]는 "박스 내부"에서의 인덱스
                // 전체 메시 기준으로는 baseIndex를 더해야 함
                triangles[triBase + i] = boxTriangles[i] + baseIndex;
            }
        }

        /// <summary>
        /// 1×1×1 크기의 정육면체를 (로컬 좌표)  -0.5~+0.5 범위로 만드는 예시 함수
        /// </summary>
        private static Vertex[] CreateUnitBox(out int[] outTriangles)
        {
            // (로컬 좌표 -0.5~+0.5)를 가지는 1×1×1 큐브
            // 아래는 RenderData.CreateCube1x1()와 유사한 방식
            Vertex[] verts = new Vertex[]
            {
            // 아래 면 (z=-0.5) 0~3
            new Vertex { Position_ObjectSpace = new Vector3(-0.5f, -0.5f, -0.5f) },
            new Vertex { Position_ObjectSpace = new Vector3( 0.5f, -0.5f, -0.5f) },
            new Vertex { Position_ObjectSpace = new Vector3( 0.5f,  0.5f, -0.5f) },
            new Vertex { Position_ObjectSpace = new Vector3(-0.5f,  0.5f, -0.5f) },

            // 위   면 (z=+0.5) 4~7
            new Vertex { Position_ObjectSpace = new Vector3(-0.5f, -0.5f,  0.5f) },
            new Vertex { Position_ObjectSpace = new Vector3( 0.5f, -0.5f,  0.5f) },
            new Vertex { Position_ObjectSpace = new Vector3( 0.5f,  0.5f,  0.5f) },
            new Vertex { Position_ObjectSpace = new Vector3(-0.5f,  0.5f,  0.5f) },
            };

            int[] tris = new int[]
            {
            // front  (z=+0.5)
            4,5,6,  4,6,7,
            // right  (x=+0.5)
            5,1,2,  5,2,6,
            // back   (z=-0.5)
            1,0,3,  1,3,2,
            // left   (x=-0.5)
            0,4,7,  0,7,3,
            // top    (y=+0.5)
            3,7,6,  3,6,2,
            // bottom (y=-0.5)
            1,5,4,  1,4,0
            };

            outTriangles = tris;
            return verts;
        }

        /// <summary>
        /// 주어진 함수 f(t)를 [startT, endT]에서 sampleCount로 샘플링하고,
        /// 인접 샘플마다 "짧은 직육면체(기둥)"를 생성하여 이어 붙인 형태의 RenderData를 만든다.
        /// </summary>
        public static RenderData CreateExtrudedLineFromFunction(
            Func<float, Vector3> f,
            float startT,
            float endT,
            int sampleCount,
            float width,  // 기둥의 '두께' (x방향)
            float height, // 기둥의 '두께' (y방향)
            Color color
        )
        {
            // 1) t를 샘플링
            Vector3[] points = new Vector3[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float ratio = i / (float)(sampleCount - 1);
                float t = startT + (endT - startT) * ratio;
                points[i] = f(t);
            }

            // 2) RenderData 기본 준비
            RenderData renderData = new RenderData();
            // 아직 정점/인덱스가 없으므로, 빈 배열로 시작
            renderData.Vertices = new Vertex[0];
            renderData.Triangles = new int[0];
            renderData.Colors = null; // (필요에 따라 세팅)

            // 3) 구간 별로 직육면체 추가
            for (int i = 0; i < sampleCount - 1; i++)
            {
                Vector3 p0 = points[i];
                Vector3 p1 = points[i + 1];
                Vector3 dir = p1 - p0;
                float length = dir.magnitude;

                if (length < 1e-6f)
                {
                    // 두 점이 거의 같은 경우, 생략
                    continue;
                }

                // (1) 중심 = (p0 + p1)/2
                Vector3 center = (p0 + p1) * 0.5f;

                // (2) dir을 정규화 (축 z 기준)
                Vector3 forward = dir.normalized;

                // (3) 어떤 up 벡터를 기준으로 회전할지 결정.
                //     - 일반적으로 (0,1,0)을 원하는 '위쪽'으로 두고,
                //       forward와 up이 평행해지는 경우를 피하기 위해 보정할 수도 있음.
                Vector3 worldUp = new Vector3(0, 1, 0);
                // cross, dot 등을 이용해 적절히 회전(Quaternion.LookRotation 등)
                Quaternion rot = Quaternion.LookDirection(forward, worldUp);

                // (4) 구간 기둥(직육면체) 하나를 메시에 합침
                AddSegment(
                    ref renderData.Vertices,
                    ref renderData.Triangles,
                    center,
                    rot,
                    length, // z축 방향 길이
                    width,  // x축 두께
                    height, // y축 두께
                    color
                );
            }

            // 4) 노말/바운딩박스 계산
            renderData.CalculateNormals();
            renderData.CalculateAABB();

            // 5) 완료
            return renderData;
        }
    }
}
