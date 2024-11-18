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
        public List<PBRObject> Targets;
        public float[,] ZBuffer; // Z-버퍼 추가
        public float[] ZBuffer2; // Z-버퍼 추가

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
            Parallel.For(0, ZBuffer2.Length, (i) =>
            {
                ZBuffer2[i] = float.MaxValue;
            });
        }
        public void AddObject(PBRObject obj)
        {
            Targets.Add(obj);
            R = new Rasterizer(Targets[0].Vertices2.Length, ZBuffer2.Length, ZBuffer2.Length, Targets[0].Triangles.Length, width, height);
        }
        public PBRRenderer()
        {
            Targets = new List<PBRObject>();
            vShader = new VertexShader();
        }
        Rasterizer R;
        VertexShader vShader;
        public void Render()
        {
            Matrix4x4 cmaeraTransform = camera.CalculatePerspectiveProjectionMatrix();
            RenderTarget.ClearBlack();
            ClearZBuffer();
            Vector3 light = new Vector3(1, -1, 1); // 광원 방향 정의
            light = light.normalized; // 광원 방향 벡터를 정규화
            //Vector3 lightInCameraSpace = TransformMatrixCaculator.Transform(light.normalized, cmaeraTransform).normalized; // 광원을 카메라 좌표계로 변환


            foreach (var mesh in Targets)
            {
                Matrix4x4 objectTransform = mesh.CalculateObjectTransformMatrix();

                Matrix4x4 transform = cmaeraTransform * objectTransform;

                Vertex[] transformedVertices = vShader.Run(mesh.Vertices2, objectTransform, cmaeraTransform);
                transformedVertices = mesh.Shader.Run_VertexShader(transformedVertices, mesh.Position);
                transformedVertices = vShader.Run2(mesh.Vertices2, objectTransform, cmaeraTransform);
                var s = R.RunTiled(transformedVertices, mesh.FragmentShader, ZBuffer2, RenderTarget, mesh.Triangles, width, height);
                var t =  mesh.Shader.Run_FragmentShader(s, RenderTarget.Pixels, width);
                RenderTarget.SetPixels(t);
            }
        }


    public void RasterizeTriangle(Vertex p1, Vertex p2, Vertex p3, NPhotoshop.Core.Image.Color color)
        {
            Vector3 pt1 = p1.Position_ScreenVolumeSpace;
            Vector3 pt2 = p2.Position_ScreenVolumeSpace;
            Vector3 pt3 = p3.Position_ScreenVolumeSpace;
            // 삼각형의 꼭짓점 좌표를 정렬 (y축 기준)
            if (pt2.y < pt1.y) (pt1, pt2) = (pt2, pt1);
            if (pt3.y < pt1.y) (pt1, pt3) = (pt3, pt1);
            if (pt3.y < pt2.y) (pt2, pt3) = (pt3, pt2);

            // 삼각형의 꼭짓점 간의 경사율 계산
            float invslope1 = (pt2.x - pt1.x) / (pt2.y - pt1.y);
            float invslope2 = (pt3.x - pt1.x) / (pt3.y - pt1.y);

            // 상단 삼각형 부분 스캔
            for (float y = pt1.y; y <= pt2.y; y++)
            {
                float x1 = pt1.x + (y - pt1.y) * invslope1;
                float x2 = pt1.x + (y - pt1.y) * invslope2;
                if (x1 > x2) (x1, x2) = (x2, x1);
                for (float x = x1; x <= x2; x++)
                {
                    SetPixelWithZBuffer((int)x, (int)y, p1, p2, p3, color);
                }
            }

            // 하단 삼각형 부분 스캔
            invslope1 = (pt3.x - pt2.x) / (pt3.y - pt2.y);
            for (float y = pt2.y; y <= pt3.y; y++)
            {
                float x1 = pt2.x + (y - pt2.y) * invslope1;
                float x2 = pt1.x + (y - pt1.y) * invslope2;
                if (x1 > x2) (x1, x2) = (x2, x1);
                for (float x = x1; x <= x2; x++)
                {
                    SetPixelWithZBuffer((int)x, (int)y, p1, p2, p3, color);
                }
            }
        }

        // Z-버퍼와 함께 픽셀을 그리는 함수
        private void SetPixelWithZBuffer(int x, int y, Vertex p1, Vertex p2, Vertex p3, NPhotoshop.Core.Image.Color color)
        {
            if (x < 0 || x >= ZBuffer.GetLength(0))
                return;
            if (y < 0 || y >= ZBuffer.GetLength(1))
                return;
            Vector3 pt1 = p1.Position_ScreenVolumeSpace;
            Vector3 pt2 = p2.Position_ScreenVolumeSpace;
            Vector3 pt3 = p3.Position_ScreenVolumeSpace;

            // 바리센트릭 좌표 계산을 위한 삼각형 면적 계산
            float areaABC = EdgeFunction(pt1, pt2, pt3);
            if (areaABC == 0.0f) return; // 삼각형 면적이 0일 때는 무시

            // 현재 픽셀에서의 바리센트릭 좌표 계산
            Vector3 p = new Vector3(x, y, 0);

            // 픽셀 P에 대한 각 꼭짓점의 기여도 (바리센트릭 좌표)
            float lambda1 = EdgeFunction(pt2, pt3, p) / areaABC;
            float lambda2 = EdgeFunction(pt3, pt1, p) / areaABC;
            float lambda3 = EdgeFunction(pt1, pt2, p) / areaABC;

            // 깊이 값을 바리센트릭 좌표를 통해 보간
            float zInterpolated = lambda1 * pt1.z + lambda2 * pt2.z + lambda3 * pt3.z;
            Vector3 normalInterpolated = lambda1 * p1.Normal_WorldSpace + lambda2 * p2.Normal_WorldSpace + lambda3 * p3.Normal_WorldSpace;

            // Z-버퍼 비교 및 픽셀 설정
            if (ZBuffer[x, y] <= zInterpolated)
            {
                ZBuffer[x, y] = zInterpolated;
                float brightness = Vector3.Dot(normalInterpolated.normalized, (new Vector3(-1f, -1, 0)).normalized);
                //brightness = System.Math.Clamp(brightness, 0.0f, 1.0f); // 음수는 0으로, 1을 넘지 않도록 클램프
                brightness = 0.5f * brightness + 0.5f;
                byte intensity = (byte)(brightness * 255);
                color = new NPhotoshop.Core.Image.Color(intensity, intensity, intensity, 255);

                RenderTarget.SetPixel(x, y, color);
            }
        }
        // 삼각형의 꼭짓점으로 면적(에지 함수)을 계산하는 함수
        private float EdgeFunction(Vector3 a, Vector3 b, Vector3 c)
        {
            // 삼각형의 면적을 구하는 에지 함수
            return (c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x);
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
