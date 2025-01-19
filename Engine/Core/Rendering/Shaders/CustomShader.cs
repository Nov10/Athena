using Athena.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Engine.Core.Image;
using ILGPU;
using ILGPU.Runtime;

namespace Athena.Engine.Core.Rendering.Shaders
{
    public abstract class CustomShader
    {
        //public Color[] Run_FragmentShader(Raster[] rasters, Color[] frameBuffer, Vector3 lightDir, int width)
        //{
        //    Parallel.For(0, rasters.Length, (idx) =>
        //    {
        //        if (rasters[idx].TriangleIndex != -1)
        //            frameBuffer[rasters[idx].x + rasters[idx].y * width] = FragmentShader(rasters[idx], lightDir);
        //    });
        //    return frameBuffer;
        //}

        public abstract void RunVertexShader_GPU(MemoryBuffer1D<Vertex, Stride1D.Dense> vertices, Vector3 objectPosition_WS, int length);
        public abstract void RunFragmentShader_GPU(MemoryBuffer1D<Raster, Stride1D.Dense> rasters, MemoryBuffer1D<Color, Stride1D.Dense> framebuffer, Vector3 lightDirection, int width);

        //public abstract Color FragmentShader(Raster raster, Vector3 light);
        //public abstract Vector3 VertextShader(Vector3 vertex_position_WorldSpace, Vector3 vertex_normal_WorldSpace, Vector3 objectposition_WorldSpace);
    }
}
