using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Engine.Core.Image;
using Athena.Maths;
using ILGPU;
using ILGPU.Runtime;
using static Athena.Engine.Core.Rendering.Shaders.NormalShader;

namespace Athena.Engine.Core.Rendering.Shaders
{
    public class UnlitColorShader : CustomShader
    {
        Color ThisColor { get { return Data.MainColor; } set { Data.MainColor = value; } }  
        UnlitColorShaderData Data;
        public MemoryBuffer1D<Color, Stride1D.Dense> buffer;
        Action<Index1D, ArrayView<Raster>, ArrayView<Color>, UnlitColorShaderData, int> Kernel_FragmentShader;
        public UnlitColorShader(Color c)
        {
            ThisColor = c;
            Kernel_FragmentShader = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<Raster>, ArrayView<Color>, UnlitColorShaderData, int>(GPUFragment);
        }
        public void SetColor(Color c)
        {
            ThisColor = c;
        }
        public struct UnlitColorShaderData
        {
            public Color MainColor;
        }

        static void GPUFragment(Index1D idx, ArrayView<Raster> rasters, ArrayView<Color> framebuffer, UnlitColorShaderData data, int width)
        {
            if (rasters[idx].TriangleIndex != -1)
            {
                framebuffer[rasters[idx].x + rasters[idx].y * width] = data.MainColor;
            }
        }

        public override void RunVertexShader_GPU(MemoryBuffer1D<Vertex, Stride1D.Dense> vertices, Vector3 objectPosition_WS)
        {
        }

        public override void RunFragmentShader_GPU(MemoryBuffer1D<Raster, Stride1D.Dense> rasters, MemoryBuffer1D<Color, Stride1D.Dense> framebuffer, Vector3 lightDirection, int width)
        {
            Kernel_FragmentShader((int)rasters.Length, rasters.View, framebuffer.View, this.Data, width);
        }
    }
}
