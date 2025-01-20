using Assimp;
using Athena.Engine.Core;
using Athena.Engine.Core.Image;
using Athena.Engine.Core.Rendering.Lights;
using Athena.Maths;
using System;
using System.Collections.Generic;

namespace Athena.Engine.Core.Rendering
{
    /*
     * 렌더링의 흐름은 다음과 같다.
     * 
     * Renderer 생성 : PBR, RayTracing...
     * Camera 생성, Camera를 Renderer에 등록
     * 
     * 예를 들어,
     * Camera1 -> PBRRender
     * Camera2 -> RayTracing
     * Camera3 -> PBRRender
     * 
     * 이후 모든 Renderer에 대해 렌더링을 실행한다.
     * 각 Renderer는 미리 등록된 Camera를 대상으로 렌더링을 실행하며, 그 결과는 각 Camera 내부의 RenderTarget에 저장된다.
     * 
     * 즉,
     * PBRRenderer : Camera1 렌더링 -> Camera3 렌더링
     * RaTracing : Camera2 렌더링
     */
    public abstract class BaseRenderer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        List<Camera> Cameras;
        public BaseRenderer(int w, int h)
        {
            Width = w;
            Height = h;
            Cameras = new List<Camera>();
        }

        public static void RegisterCameraToRenderer(Camera camera, BaseRenderer renderer)
        {
            //이것만큼은 순환참조를 피해야 할거 같아서...
            foreach(var r in EngineController.Renderers)
            {
                r.Cameras.Remove(camera);
            }

            renderer.Cameras.Add(camera);
        }


        public void Render(List<MeshRenderer> targets, List<Lights.Light> lights)
        {
            for(int i = 0; i < Cameras.Count; i++)
            {
                if (Width != Cameras[i].RenderTarget.Width || Height != Cameras[i].RenderTarget.Height)
                {
                    throw new System.Exception($"Width and Height mismatch with Renderer and RenderTarget. Renderer({Width}, {Height}) / RenderTarget({Cameras[i].RenderTarget.Width}, {Cameras[i].RenderTarget.Height})");
                }
                EngineController.DLight.Controller.WorldPosition = Cameras[i].Controller.WorldPosition + -EngineController.DLight.Controller.WorldRotation.RotateVectorZDirection() * 50;
                EngineController.DLight.RenderShadowMap(Athena.Engine.Core.MeshRenderer.RendererList);
                InternelRender(Cameras[i], targets, lights);
            }
        }

        protected abstract void InternelRender(Camera camera, List<MeshRenderer> targets, List<Lights.Light> lights);
    }
}
