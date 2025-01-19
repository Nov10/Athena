using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU;
using Athena.Engine.Core.Image;
using ILGPU.Runtime;
using static Athena.Engine.Core.Rendering.Shaders.NormalShader;

namespace Athena.Engine.Core.Rendering.Shaders
{
    public class ShaderKernel<TVertexShader, TFragmentShader> 
        where TVertexShader : struct
        where TFragmentShader : struct
    {
        Action<Index1D, ArrayView<Raster>, ArrayView<Color>, TFragmentShader, int> Kernel_FragmentShader;
        Action<Index1D, ArrayView<Vertex>, TVertexShader> Kernel_VertexShader;

        public ShaderKernel(
            System.Action<Index1D, ArrayView<Vertex>, TVertexShader> GPUVertex,
            System.Action<Index1D, ArrayView<Raster>, ArrayView<Color>, TFragmentShader, int> GPUFragment)
        {
            Kernel_FragmentShader = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<Raster>, ArrayView<Color>, TFragmentShader, int>(GPUFragment);
            Kernel_VertexShader = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<Vertex>, TVertexShader>(GPUVertex);
        }

        public void Run_FragmentKernel(MemoryBuffer1D<Raster, Stride1D.Dense> rasters, MemoryBuffer1D<Color, Stride1D.Dense> framebuffer, int width, TFragmentShader t)
        {
            Kernel_FragmentShader((int)rasters.Length, rasters.View, framebuffer.View, t, width);
        }

        public void Run_VertexKernel(MemoryBuffer1D<Vertex, Stride1D.Dense> vertex, TVertexShader t, int length)
        {
            Kernel_VertexShader(length, vertex.View, t);
        }
    }
}
