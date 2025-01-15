using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using Athena.Engine.Core.Image;

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

        public override Color FragmentShader(Raster raster, Vector3 light)
        {
            var normal = raster.Normal_WorldSpace;
            float brightness = Vector3.Dot(normal.normalized, light);
            brightness = (brightness + 1) * 0.5f;

            var color = ThisColor * brightness;
            color.A = 255;
            return color;
        }

        public override Vector3 VertextShader(Vector3 vertex_position_WorldSpace, Vector3 vertex_normal_WorldSpace, Vector3 objectposition_WorldSpace)
        {
            return vertex_position_WorldSpace;
        }
    }
}
