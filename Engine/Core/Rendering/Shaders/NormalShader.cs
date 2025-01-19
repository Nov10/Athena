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

namespace Athena.Engine.Core.Rendering.Shaders
{
    public class NormalShader : CustomShader
    {
        public Texture2DWrapper MainTexture;
        NormalShaderData FragmentShaderData;
        VertexData VertexShaderData;

        ShaderKernel<VertexData, NormalShaderData> Kernels;
        public struct NormalShaderData
        {
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
                float brightness = Vector3.Dot(normal, -(new Vector3(0, -2, -1)).normalized) * 0.5f + 0.5f;
                framebuffer[rasters[idx].x + rasters[idx].y * width] = ShaderHelper.SampleTexture_GPU(data.MainTexture, rasters[idx].UV)  * brightness;
            }
        }

        static void GPUVertex(Index1D idx, ArrayView<Vertex> vertices, VertexData data)
        {

        }

        public override void RunFragmentShader_GPU(MemoryBuffer1D<Raster, Stride1D.Dense> rasters, MemoryBuffer1D<Color, Stride1D.Dense> framebuffer, Vector3 lightDirection, int width)
        {
            Kernels.Run_FragmentKernel(rasters, framebuffer, width, this.FragmentShaderData);
        }
    }
}
