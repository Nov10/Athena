﻿using Athena.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.UI.Xaml.Media.Imaging;
using ILGPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime;
using System.Diagnostics;
using Athena.Engine.Core.Image;
using Athena.Engine.Core.Rendering;

namespace Athena.Engine.Core.Rendering
{
    public class PBRRenderer : BaseRenderer
    {
        public Vector3 LightDirection;
        GPURasterizer Rasterizer;
        VertexShader VertexShader;

        public PBRRenderer(int w, int h) : base(w, h)
        {
            VertexShader = new VertexShader();
            Rasterizer = new GPURasterizer(Width, Height);
        }

        protected override void InternelRender(Camera camera, List<MeshRenderer> targets)
        {
            Matrix4x4 VP = camera.CalculateVPMatrix();
            camera.RenderTarget.Clear(new Core.Image.Color(0, 255, 255, 255));

            //Vector3 lightInCameraSpace = TransformMatrixCaculator.Transform(light.normalized, cmaeraTransform).normalized; // 광원을 카메라 좌표계로 변환
            Rasterizer.Start();
            foreach (var renderer in targets)
            {
                if (renderer.Controller == null)
                    continue;
                if (renderer.Controller.IsWorldActive == false)
                    continue;

                Matrix4x4 M = renderer.CreateObjectTransformMatrix();
                Matrix4x4 objectRotationTransform = renderer.CreateObjectRotationMatrix();
                Matrix4x4 objectInvTransform = TransformMatrixCaculator.CreateObjectInvTransformMatrix(renderer.Controller);
                Matrix4x4 MVP = VP * M;

                foreach (var data in renderer.RenderDatas)
                {
                    if (data.Vertices == null || data.Vertices.Length == 0)
                        continue;

                    if (FrustumCulling.Culling(data.ThisAABB, camera.Controller, renderer.Controller, MVP, objectInvTransform) == false)                    
                        continue;
                    
                    //Object -> World -> ClipSpace
                    Vertex[] transformedVertices = VertexShader.Run(data.Vertices, renderer.Controller.WorldPosition, data.Shader, M, VP, objectRotationTransform);
                    //ClipSpace -> NDC -> Raster
                    var rasters = Rasterizer.Run(transformedVertices, data.Triangles, Width, Height);

                    if (rasters == null)
                        continue;

                    var frameBuffer = data.Shader.Run_FragmentShader(rasters, camera.RenderTarget.GetPixels(), LightDirection, Width);
                    camera.RenderTarget.SetPixels(frameBuffer, 0);
                }
            }
        }
    }
}
