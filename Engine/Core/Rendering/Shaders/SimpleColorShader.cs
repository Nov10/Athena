using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using Athena.Engine.Core.Image;
using ILGPU.Runtime;
using ILGPU;
using Athena.Engine.Core.Rendering.Lights;

namespace Athena.Engine.Core.Rendering.Shaders
{
    public class SimpleColorShader : CustomShader
    {
        Color ThisColor;
        public SimpleColorShader(Color c)
        {
            SetColor(c);
        }
        public void SetColor(Color c)
        {
            ThisColor = c;
        }

        //public override Color FragmentShader(Raster raster, Vector3 light)
        //{
        //    var normal = raster.Normal_WorldSpace;
        //    float brightness = Vector3.Dot(normal.normalized, light);
        //    brightness = (brightness + 1) * 0.5f;

        //    var color = ThisColor * brightness;
        //    color.A = 255;
        //    return color;
        //}

        public override void RunVertexShader_GPU(MemoryBuffer1D<Vertex, Stride1D.Dense> vertices, Vector3 objectPosition_WS, int length)
        {
        }

        public override void RunFragmentShader_GPU(MemoryBuffer1D<Raster, Stride1D.Dense> rasters, MemoryBuffer1D<Color, Stride1D.Dense> framebuffer, Light[] datas, int width)
        {
        }
    }
}
