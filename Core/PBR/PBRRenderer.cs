using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renderer;
using System.Drawing;
using Microsoft.UI.Xaml.Media.Imaging;
using NPhotoshop.Core.Image;

namespace Renderer.Renderer.PBR
{
    public class PBRRenderer : BaseRenderer
    {
        public NBitmap RenderTarget;
        //public Bitmap RenderTarget;
        public List<Core.Renderer> Targets;
        public float[] ZBuffer; // Z-버퍼 추가
        public Vector3 LightDirection;

        public void ClearZBuffer()
        {
            //for (int x = 0; x < ZBuffer.GetLength(0); x++)
            //{
            //    for (int y = 0; y < ZBuffer.GetLength(1); y++)
            //    {
            //        ZBuffer[x, y] = -float.MaxValue; // 무한대로 초기화
            //    }
            //}
            //for(int i = 0; i < ZBuffer2.Length; i++)
            Parallel.For(0, ZBuffer.Length, (i) =>
            {
                ZBuffer[i] = float.MaxValue;
            });
        }
        public void AddObject(Core.Renderer obj)
        {
            Targets.Add(obj);
        }

        public PBRRenderer(int w, int h) : base(w, h)
        {
            Targets = new List<Core.Renderer>();
            VertexShader = new VertexShader();
            Rasterizer = new Rasterizer(width, height);

            RenderTarget = new NPhotoshop.Core.Image.NBitmap(width, height);
            ZBuffer = new float[width * height];
        }
        Rasterizer Rasterizer;
        VertexShader VertexShader;

        private static List<Vertex[]> ClipTriangleAgainstNearZ(Vertex v1, Vertex v2, Vertex v3, float nearZ)
        {
            // 1) 각 정점이 nearZ보다 앞(IN)인지, 뒤(OUT)인지 판별
            bool in1 = (v1.Position_WorldSpace.z <= nearZ);
            bool in2 = (v2.Position_WorldSpace.z <= nearZ);
            bool in3 = (v3.Position_WorldSpace.z <= nearZ);

            int inCount = (in1 ? 1 : 0) + (in2 ? 1 : 0) + (in3 ? 1 : 0);

            // 모두 OUT이면 -> 아무것도 반환 안 함
            if (inCount == 0)
                return new List<Vertex[]>();

            // 모두 IN이면 -> 그대로 삼각형 유지
            if (inCount == 3)
                return new List<Vertex[]>() { new Vertex[] { v1, v2, v3 } };

            // 섞여있는 경우 -> 교차점 찾아서 새 삼각형(들) 생성
            List<Vertex> clipped = new List<Vertex>();

            // vA->vB를 클리핑하는 헬퍼 (near-plane과의 교차점 등 계산)
            void ClipEdge(Vertex a, Vertex b, bool aIn, bool bIn)
            {
                if (aIn && bIn)
                {
                    // 둘 다 IN이면 b만 추가
                    clipped.Add(b);
                }
                else if (aIn && !bIn)
                {
                    // a in, b out -> 교차점만 추가
                    Vertex interV = IntersectNearPlane(a, b, nearZ);
                    clipped.Add(interV);
                }
                else if (!aIn && bIn)
                {
                    // a out, b in -> 교차점 + b
                    Vertex interV = IntersectNearPlane(a, b, nearZ);
                    clipped.Add(interV);
                    clipped.Add(b);
                }
                // 둘 다 OUT이면 추가 없음
            }

            // 세 에지: v1->v2, v2->v3, v3->v1
            ClipEdge(v1, v2, in1, in2);
            ClipEdge(v2, v3, in2, in3);
            ClipEdge(v3, v1, in3, in1);

            // clipped에는 0,3,4개 정점이 들어있을 수 있음
            // 3개 => 삼각형 1개
            // 4개 => 사다리꼴 => 삼각형 2개
            List<Vertex[]> result = new List<Vertex[]>();
            if (clipped.Count < 3)
            {
                // 0 ~ 2개면 삼각형 안 됨
                return result;
            }
            else if (clipped.Count == 3)
            {
                result.Add(new Vertex[] { clipped[0], clipped[1], clipped[2] });
            }
            else // clipped.Count == 4
            {
                // (0,1,2), (0,2,3) 2개 삼각형으로 분할
                result.Add(new Vertex[] { clipped[0], clipped[1], clipped[2] });
                result.Add(new Vertex[] { clipped[0], clipped[2], clipped[3] });
            }

            return result;
        }

        /// <summary>
        /// 두 정점 a->b가 z=nearZ와 교차하는 점을 찾아 보간(위치, 노말, UV 등) 반환
        /// </summary>
        private static Vertex IntersectNearPlane(Vertex a, Vertex b, float nearZ)
        {
            Vector3 pA = a.Position_WorldSpace;
            Vector3 pB = b.Position_WorldSpace;
            float t = (nearZ - pA.z) / (pB.z - pA.z);

            Vertex vOut = new Vertex();

            // 위치 보간
            vOut.Position_WorldSpace = pA + t * (pB - pA);
            // UV 등도 동일하게 보간
            vOut.UV = a.UV + t * (b.UV - a.UV);

            // 노말도 보간 후 정규화 (혹은 보간 후 normalize)
            Vector3 nA = a.Normal_WorldSpace;
            Vector3 nB = b.Normal_WorldSpace;
            vOut.Normal_WorldSpace = (nA + t * (nB - nA)).normalized;

            // Tangent, Bitangent 등 필요한 속성도 동일하게 보간
            // (a.Tangent + t * (b.Tangent - a.Tangent)), etc.

            return vOut;
        }
        public static void ClipTrianglesByNearPlane(
    ref Vertex[] vertices,
    ref int[] triangles,
    float nearZ)
        {
            List<Vertex[]> clippedTriList = new List<Vertex[]>();

            // 기존 index 기반으로 삼각형을 순회
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vertex v1 = vertices[triangles[i]];
                Vertex v2 = vertices[triangles[i + 1]];
                Vertex v3 = vertices[triangles[i + 2]];

                // 삼각형 하나씩 클리핑
                var subTris = ClipTriangleAgainstNearZ(v1, v2, v3, nearZ);
                // 반환된 삼각형들을 모은다
                clippedTriList.AddRange(subTris);
            }

            // 이제 clippedTriList 에는 (최대 2*원본삼각형수) 개의 '새 삼각형'이
            // 각각 Vertex[3] 형태로 들어 있음.
            // => 우리가 다시 새 vertices[], 새 triangles[] 로 만들어야 함.

            // (1) 간단하게 각 삼각형이 독립된 3개 정점을 쓰도록 "모두 분리"해버리는 예시:
            //     -> 중복 정점이라도 전부 따로 둔다.(간단하지만 비효율적)
            List<Vertex> newVertexList = new List<Vertex>();
            List<int> newIndexList = new List<int>();

            foreach (var tri in clippedTriList)
            {
                // tri는 Vertex[3]
                newVertexList.Add(tri[0]);
                newVertexList.Add(tri[1]);
                newVertexList.Add(tri[2]);

                int baseIdx = newVertexList.Count - 3;
                newIndexList.Add(baseIdx);
                newIndexList.Add(baseIdx + 1);
                newIndexList.Add(baseIdx + 2);
            }

            // 최종 배열로 교체
            vertices = newVertexList.ToArray();
            triangles = newIndexList.ToArray();
        }
        public void Render()
        {
            Matrix4x4 cameraTransform = camera.CalculateRenderMatrix();
            RenderTarget.ClearBlack();
            ClearZBuffer();
            //Vector3 lightInCameraSpace = TransformMatrixCaculator.Transform(light.normalized, cmaeraTransform).normalized; // 광원을 카메라 좌표계로 변환


            foreach (var mesh in Targets)
            {
                Matrix4x4 objectTransform = mesh.CalculateObjectTransformMatrix();
                Matrix4x4 objectRotationTransform = mesh.CalculateObjectRotationMatrix();
                Matrix4x4 transform = cameraTransform * objectTransform;
                foreach(var m in mesh.RenderDatas)
                {
                    var singleMesh = new RenderData(m);
                    //물체의 위치, 각도 적용
                    Vertex[] transformedVertices = VertexShader.Run(singleMesh.Vertices2, mesh.Controller.LocalPosition, singleMesh.Shader, objectTransform, cameraTransform, objectRotationTransform);
                    //VertexShader.Calc_T(transformedVertices, singleMesh.Triangles);
                    //래스터 계산
                    var rasters = Rasterizer.Run(transformedVertices, ZBuffer, RenderTarget, singleMesh.Triangles, width, height);
                    //프래그먼트 셰이더로 색상 계산
                    var frameBuffer = singleMesh.Shader.Run_FragmentShader(rasters, RenderTarget.Pixels, LightDirection, width);
                    RenderTarget.SetPixels(frameBuffer);
                }
            }
        }

        // 클립 코드 상수
        const int INSIDE = 0; // 0000
        const int LEFT = 1;   // 0001
        const int RIGHT = 2;  // 0010
        const int BOTTOM = 4; // 0100
        const int TOP = 8;    // 1000

        // 클리핑 영역 좌표 (window boundaries)
        const int x_min = 0;
        const int y_min = 0;
        private int ComputeClipCode(int x, int y)
        {
            int x_max = width - 1;
            int y_max = height - 1;
            int code = INSIDE;

            if (x < x_min) // 좌측
                code |= LEFT;
            else if (x > x_max) // 우측
                code |= RIGHT;
            if (y < y_min) // 아래쪽
                code |= BOTTOM;
            else if (y > y_max) // 위쪽
                code |= TOP;

            return code;
        }// Cohen-Sutherland 클리핑 알고리즘
        private bool CohenSutherlandClipLine(ref int x0, ref int y0, ref int x1, ref int y1)
        {
            int code0 = ComputeClipCode(x0, y0);
            int code1 = ComputeClipCode(x1, y1);

            bool accept = false;

            while (true)
            {
                if ((code0 | code1) == 0)
                {
                    // 둘 다 클리핑 영역 안에 있음
                    accept = true;
                    break;
                }
                else if ((code0 & code1) != 0)
                {
                    // 둘 다 클리핑 영역 밖에 있음 (밖에서 교차하지 않음)
                    break;
                }
                else
                {
                    // 한 점은 클리핑 영역 안에 있고 다른 한 점은 밖에 있음, 교차점을 찾음
                    int codeOut = code0 != 0 ? code0 : code1;
                    int x, y;

                    int x_max = width - 1;
                    int y_max = height - 1;
                    if ((codeOut & TOP) != 0)
                    {
                        x = x0 + (x1 - x0) * (y_max - y0) / (y1 - y0);
                        y = y_max;
                    }
                    else if ((codeOut & BOTTOM) != 0)
                    {
                        x = x0 + (x1 - x0) * (y_min - y0) / (y1 - y0);
                        y = y_min;
                    }
                    else if ((codeOut & RIGHT) != 0)
                    {
                        y = y0 + (y1 - y0) * (x_max - x0) / (x1 - x0);
                        x = x_max;
                    }
                    else
                    {
                        y = y0 + (y1 - y0) * (x_min - x0) / (x1 - x0);
                        x = x_min;
                    }

                    if (codeOut == code0)
                    {
                        x0 = x;
                        y0 = y;
                        code0 = ComputeClipCode(x0, y0);
                    }
                    else
                    {
                        x1 = x;
                        y1 = y;
                        code1 = ComputeClipCode(x1, y1);
                    }
                }
            }

            return accept;
        }

        private void DrawLine(int x0, int y0, int x1, int y1, NPhotoshop.Core.Image.Color color)
        {
            // Cohen-Sutherland 클리핑 알고리즘을 통해 선이 클리핑 영역 안에 있는지 확인
            if (CohenSutherlandClipLine(ref x0, ref y0, ref x1, ref y1))
            {
                int dx = System.Math.Abs(x1 - x0);
                int dy = System.Math.Abs(y1 - y0);
                int sx = x0 < x1 ? 1 : -1;
                int sy = y0 < y1 ? 1 : -1;
                int err = dx - dy;

                while (true)
                {
                    if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
                    {
                        RenderTarget.SetPixel(x0, y0, color);
                    }

                    if (x0 == x1 && y0 == y1) break;
                    int e2 = 2 * err;
                    if (e2 > -dy) { err -= dy; x0 += sx; }
                    if (e2 < dx) { err += dx; y0 += sy; }
                }
            }
        }
    }
}
