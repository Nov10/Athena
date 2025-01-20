using Athena.Engine;
using Athena.Engine.Core;
using Athena.Engine.Core.Image;
using Athena.Engine.Core.Rendering.Lights;
using Athena.Engine.Helpers;
using Athena.Maths;
using ILGPU;
using ILGPU.Runtime;

namespace Athena.Engine.Core.Rendering.Shaders
{
    public class ShadowMapShader : CustomShader
    {
        FragmentShaderData Data_FRG;
        VertexShaderData Data_VTX;
        ShaderKernel<VertexShaderData, FragmentShaderData> Kernels;

        struct FragmentShaderData
        {
        }
        struct VertexShaderData
        {
             public Vector3 ObjectPosition_WS;
        }

        public ShadowMapShader()
        {
            Kernels = new ShaderKernel<VertexShaderData, FragmentShaderData>(Kernel_VertexShader, Kernel_FragmentShader);
            this.Data_FRG = new FragmentShaderData();
            this.Data_VTX = new VertexShaderData();
        }

        public override void RunFragmentShader_GPU(MemoryBuffer1D<Raster, Stride1D.Dense> rasters, MemoryBuffer1D<Color, Stride1D.Dense> framebuffer, Light[] lights, int width)
        {
            Kernels.Run_FragmentKernel(rasters, framebuffer, width, this.Data_FRG);
        }
        public override void RunVertexShader_GPU(MemoryBuffer1D<Vertex, Stride1D.Dense> vertices, Vector3 objectPosition_WS, int length)
        {
            this.Data_VTX.ObjectPosition_WS = objectPosition_WS;
            Kernels.Run_VertexKernel(vertices, this.Data_VTX, length);
        }

        static void Kernel_FragmentShader(Index1D idx, ArrayView<Raster> rasters, ArrayView<Color> framebuffer, FragmentShaderData data, int width)
        {
            if (rasters[idx].TriangleIndex != -1)
            {
                //framebuffer[ShaderHelper.CalculateFrameBufferIndexOfRaster(rasters[idx], width)] = rasters[idx].Depth;
            }
        }

        static void Kernel_VertexShader(Index1D idx, ArrayView<Vertex> vertices, VertexShaderData data)
        {

        }
    }
}
