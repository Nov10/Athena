using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using Athena.Engine.Core.Image;
using Athena.Engine.Core.Rendering;

namespace Athena.Engine.Core.Rendering.Shaders
{
    public class SimpleColorShader : CustomShader
    {
        Color ThisColor;
        public SimpleColorShader(Color c)
        {
            ThisColor = c;
        }
        public void SetColor(Color c)
        {
            ThisColor = c;
        }

        public override Color FragmentShader(Raster raster, Vector3 light)
        {
            var normal = raster.Normal_WorldSpace;
            float brightness = Vector3.Dot(normal.normalized, light);
            brightness = (brightness + 1) * 0.5f;
            //brightness = 0.5f * brightness + 0.5f;
            //byte intensity = (byte)(brightness * 255);
            //var c2 = ShaderHelper.SampleTexture(MainTexture, uv) * brightness;

            //raster.Tangent, raster.BitTangent, raster.Normal_WorldSpace
            var c2 = ThisColor * brightness;
            c2.A = 255;
            return c2;
        }

        public override Vector3 VertextShader(Vector3 vertex_position_WorldSpace, Vector3 vertex_normal_WorldSpace, Vector3 objectposition_WorldSpace)
        {
            return vertex_position_WorldSpace;
        }
    }
}
