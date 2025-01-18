using Athena.Maths;
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
                    using var devVertices = GPUAccelator.Accelerator.Allocate1D<Vertex>(data.Vertices.Length);
                    devVertices.CopyFromCPU(data.Vertices);

                    using var devTriangles = GPUAccelator.Accelerator.Allocate1D<int>(data.Triangles.Length);
                    devTriangles.CopyFromCPU(data.Triangles);

                    //Object -> World -> ClipSpace
                    VertexShader.Run(devVertices, renderer.Controller.WorldPosition, data.Shader, M, VP, objectRotationTransform);

                    Vertex[] v = new Vertex[data.Vertices.Length];
                    int[] t = new int[data.Triangles.Length];
                    devVertices.CopyToCPU(v);
                    devTriangles.CopyToCPU(t);

                    Vertex[] v_clippped;
                    int[] t_clippped;
                    (v_clippped, t_clippped) = GPURasterizer.ClipTriangles(v, t);
                    if (v_clippped.Length == 0)
                        continue;

                    using var devVertices2 = GPUAccelator.Accelerator.Allocate1D<Vertex>(v_clippped.Length);
                    using var devTriangles2 = GPUAccelator.Accelerator.Allocate1D<int>(t_clippped.Length);

                    devVertices2.CopyFromCPU(v_clippped);
                    devTriangles2.CopyFromCPU(t_clippped);

                    //ClipSpace -> NDC -> Raster
                    var rasters = Rasterizer.Run(devVertices2, devTriangles2, Width, Height, data.Shader);

                    if (rasters == null)
                        continue;

                    //var frameBuffer = data.Shader.Run_FragmentShader(rasters, camera.RenderTarget.GetPixels(), LightDirection, Width);
                    camera.RenderTarget.SetPixels(rasters, 0);
                }
            }
        }
    }
}
