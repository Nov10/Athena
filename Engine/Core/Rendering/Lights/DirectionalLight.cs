using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Engine.Core.Image;
using Athena.Engine.Core.Rendering.Shaders;
using Athena.Maths;
using ILGPU.Runtime;

namespace Athena.Engine.Core.Rendering.Lights
{

    public enum LightType
    {
        Directional = 0
    }
    public struct DirectionalLightData
    {
        public Matrix4x4 LightViewMatrix;
        public Texture2DFloat ShadowMap;
    }
    public class DirectionalLight : Light
    {
        public override void Awake()
        {
            base.Awake();
            VertexShader = new VertexShader();
            Rasterizer = new GPURasterizer(512, 512, GPURasterizer.Mode.ShadowMap);
            ShadowMap = new RenderBitmapFloat(512, 512);
            ShadowColorMap = new RenderBitmap(512, 512);
        }


        public RenderBitmap ShadowColorMap;
        public RenderBitmapFloat ShadowMap;
        GPURasterizer Rasterizer;
        VertexShader VertexShader;
        public void RenderShadowMap(List<MeshRenderer> targets)
        {
            Type = LightType.Directional;
            Data.Direction = Controller.WorldRotation.RotateVectorZDirection();
            if(Controller.TryGetComponent<Camera>(out Camera c) == false)
            {
                Camera cameraComponent = new Camera
                {
                    NearPlaneDistance = 1f,
                    FarPlaneDistance = 1000.0f,
                    FieldOfView = 0f,
                    AspectRatio = 1
                };
                Controller.AddComponent(cameraComponent);
            }
            Controller.TryGetComponent<Camera>(out Camera camera);
            camera.SetRenderTarget(ShadowColorMap);
            Matrix4x4 VP = Or();

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
                    {
                        continue;
                    }

                    var originVertices = data.GetVerticesBuffer();
                    VertexShader.Run(originVertices, renderer.Controller.WorldPosition, data.Shader, M, VP, objectRotationTransform, new Matrix4x4(), data.Vertices.Length);
                    originVertices.CopyToCPU(data.Vertices);

                    Vertex[] clippedVerticies;
                    int[] clipppedTriangles;
                    (clippedVerticies, clipppedTriangles) = Clipper.ClipTriangles(data.Vertices, data.Triangles);

                    if (clippedVerticies.Length == 0 || clipppedTriangles.Length == 0)
                        continue;

                    using var clipppedVerticiesBuffer = GPUAccelator.Accelerator.Allocate1D<Vertex>(clippedVerticies.Length);
                    clipppedVerticiesBuffer.CopyFromCPU(clippedVerticies);
                    using var clipppedTrianglesBuffer = GPUAccelator.Accelerator.Allocate1D<int>(clipppedTriangles.Length);
                    clipppedTrianglesBuffer.CopyFromCPU(clipppedTriangles);

                    //ClipSpace -> NDC -> Raster
                    Rasterizer.Run(clipppedVerticiesBuffer, clipppedTrianglesBuffer, (int)clipppedVerticiesBuffer.Length, (int)clipppedTrianglesBuffer.Length, ShadowMap.Width, ShadowMap.Height, data.Shader, new Light[0], false);

                    var z = Rasterizer.GetZBuffer();
                    if (z == null)
                        continue;
                    ShadowMap.SetPixels(z, 0);
                }
            }
        }
        public void S()
        {

        }
        public Matrix4x4 Or()
        {
            float NearPlaneDistance = 1;
            float FarPlaneDistance = 250;
            Vector3 zAxis, yAxis, xAxis;
            (zAxis, xAxis, yAxis) = Controller.GetDirections();
            Vector3 t = -Controller.WorldPosition;
            // 2. 직교 투영 행렬(Projection) 계산
            float invRL = 1.0f / 64;
            float invTB = 1.0f / 64;
            float invFN = 1.0f / (FarPlaneDistance - NearPlaneDistance);
            Matrix4x4 ortho = new Matrix4x4(
            2f * invRL, 0f, 0f, 0,
                0f, 2f * invTB, 0f, 0,
                0f, 0f, invFN, -(NearPlaneDistance) * invFN,
                0f, 0f, 0f, 1f
            );
            Matrix4x4 VP = ortho * new Matrix4x4(
            xAxis.x, xAxis.y, xAxis.z, Vector3.Dot(xAxis, t),
            yAxis.x, yAxis.y, yAxis.z, Vector3.Dot(yAxis, t),
            zAxis.x, zAxis.y, zAxis.z, Vector3.Dot(zAxis, t),
            0, 0, 0, 1);
            return VP;
        }

        public override void Start()
        {
        }

        public override void Update()
        {
        }
    }
}
