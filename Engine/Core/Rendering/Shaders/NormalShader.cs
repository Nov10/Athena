using Assimp.Unmanaged;
using Microsoft.UI.Xaml.Media.Imaging;
using Athena.Maths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Athena.Engine.Helpers;
using Athena.Engine.Core.Image;
using ILGPU;
using ILGPU.Runtime;
using Athena.Engine.Core.Rendering.Lights;

namespace Athena.Engine.Core.Rendering.Shaders
{
    public class NormalShader : CustomShader
    {
        public Texture2DWrapper MainTexture;
        public Texture2DFloatWrapper ShadowMapWrapper;
        NormalShaderData FragmentShaderData;
        VertexData VertexShaderData;

        ShaderKernel<VertexData, NormalShaderData> Kernels;
        public struct NormalShaderData
        {
            public LightData DData;
            public DirectionalLightData ExtraDData;
            public Texture2D MainTexture;
        }
        public struct VertexData
        {
             public Vector3 objectPosition_WS;
        }

        public NormalShader()
        {
            MainTexture = new Texture2DWrapper();
            MainTexture.Apply = () =>
            {
                this.FragmentShaderData.MainTexture = MainTexture.Texture;
            };
            ShadowMapWrapper = new Texture2DFloatWrapper();
            ShadowMapWrapper.Apply = () =>
            {
                this.FragmentShaderData.ExtraDData.ShadowMap = ShadowMapWrapper.Texture;
            };
            Kernels = new ShaderKernel<VertexData, NormalShaderData>(GPUVertex, GPUFragment);
            this.FragmentShaderData = new NormalShaderData();
            this.VertexShaderData = new VertexData();
        }

        public override void RunVertexShader_GPU(MemoryBuffer1D<Vertex, Stride1D.Dense> vertices, Vector3 objectPosition_WS, int length)
        {
            this.VertexShaderData.objectPosition_WS = objectPosition_WS;
            Kernels.Run_VertexKernel(vertices, this.VertexShaderData, length);
        }

        static void GPUFragment(Index1D idx, ArrayView<Raster> rasters, ArrayView<Color> framebuffer, NormalShaderData data, int width)
        {
            if (rasters[idx].TriangleIndex != -1)
            {
                Vector3 normal = rasters[idx].Normal_WorldSpace;
                float brightness = Vector3.Dot(normal, -data.DData.Direction) * 0.5f + 0.5f;

                var clippoint = TransformMatrixCaculator.TransformH(rasters[idx].WorldPosition, data.ExtraDData.LightViewMatrix);
                Vector4 clipPos = clippoint / clippoint.w;

                //// NDC -> Screen
                var p = new Vector3((-clipPos.x + 1) * 0.5f, (-clipPos.y + 1) * 0.5f, clipPos.z);

                if (p.x > 1 || p.x < 0 || p.y > 1 || p.y < 0)
                {
                    framebuffer[ShaderHelper.CalculateFrameBufferIndexOfRaster(rasters[idx], width)] = new Color(255, 0, 0, 255);
                }
                else
                {
                    var s = ShaderHelper.SampleTexture_UVMod_GPU(data.ExtraDData.ShadowMap, new Vector2(p.x, p.y));
                
                    float k = 0;
                    if (p.z >= s)
                        k = 0.1f;
                    else
                        k = 1;
                    framebuffer[ShaderHelper.CalculateFrameBufferIndexOfRaster(rasters[idx], width)] = ShaderHelper.SampleTexture_UVMod_GPU(data.MainTexture, rasters[idx].UV) * brightness * k;
                    framebuffer[ShaderHelper.CalculateFrameBufferIndexOfRaster(rasters[idx], width)].A = 255;
                }
            }
        }

        static void GPUVertex(Index1D idx, ArrayView<Vertex> vertices, VertexData data)
        {

        }

        public override void RunFragmentShader_GPU(MemoryBuffer1D<Raster, Stride1D.Dense> rasters, MemoryBuffer1D<Color, Stride1D.Dense> framebuffer, Light[] datas, int width)
        {
            for(int i = 0; i < datas.Length; i++)
            {
                switch (datas[i].Type)
                {
                    case LightType.Directional:
                        DirectionalLight dirLight = (DirectionalLight)datas[i];
                        Texture2DHelper.ConvertFromBitmap(ShadowMapWrapper, dirLight.ShadowMap.GetAsNBitmapFloat());
                        ShadowMapWrapper.Apply();
                        this.FragmentShaderData.ExtraDData.LightViewMatrix = dirLight.Or();
                        this.FragmentShaderData.DData.Direction = dirLight.Data.Direction;
                        this.FragmentShaderData.DData.Intensity = dirLight.Data.Intensity;
                        break;
                }
            }
            Kernels.Run_FragmentKernel(rasters, framebuffer, width, this.FragmentShaderData);
        }
    }
}
