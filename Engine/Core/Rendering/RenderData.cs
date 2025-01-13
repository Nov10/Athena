using Athena.Maths;
using Athena.Engine.Core.Rendering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                renderData.Colors[i] = Color.White;
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
    }
}
