﻿using Renderer.Maths;
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
        public List<MultipleMeshObject> Targets;
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
        public void AddObject(MultipleMeshObject obj)
        {
            Targets.Add(obj);
        }
        public PBRRenderer(int w, int h) : base(w, h)
        {
            Targets = new List<MultipleMeshObject>();
            VertexShader = new VertexShader();
            Rasterizer = new Rasterizer(width, height);
        }
        Rasterizer Rasterizer;
        VertexShader VertexShader;
        public void Render()
        {
            Matrix4x4 cameraTransform = camera.CalculatePerspectiveProjectionMatrix();
            RenderTarget.ClearBlack();
            ClearZBuffer();
            Vector3 light = new Vector3(1, -1, 1); // 광원 방향 정의
            light = light.normalized; // 광원 방향 벡터를 정규화
            //Vector3 lightInCameraSpace = TransformMatrixCaculator.Transform(light.normalized, cmaeraTransform).normalized; // 광원을 카메라 좌표계로 변환


            foreach (var mesh in Targets)
            {
                Matrix4x4 objectTransform = mesh.CalculateObjectTransformMatrix();
                Matrix4x4 transform = cameraTransform * objectTransform;
                foreach(var singleMesh in mesh.Objects)
                {
                    //물체의 위치, 각도 적용
                    Vertex[] transformedVertices = VertexShader.Run_ObjectTransform(singleMesh.Vertices2, objectTransform);
                    //물체 개별의 버텍스 셰이더 적용
                    transformedVertices = singleMesh.Shader.Run_VertexShader(transformedVertices, mesh.Position);
                    //변환행렬 적용
                    transformedVertices = VertexShader.Run_CameraTransform(singleMesh.Vertices2, cameraTransform);

                    //래스터 계산
                    var rasters = Rasterizer.Run(transformedVertices, ZBuffer2, RenderTarget, singleMesh.Triangles, width, height);
                    //프래그먼트 셰이더로 색상 계산
                    var frameBuffer = singleMesh.Shader.Run_FragmentShader(rasters, RenderTarget.Pixels, width);
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
