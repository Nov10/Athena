using Athena.Engine.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Engine.Core.Image;
using Athena.Maths;

namespace Athena.Engine.Core.Rendering.Shaders
{
    public class UnlitColorShader : CustomShader
    {
        Color ThisColor;
        public UnlitColorShader(Color c)
        {
            ThisColor = c;
        }
        public void SetColor(Color c)
        {
            ThisColor = c;
        }

        public override Color FragmentShader(Raster raster, Vector3 light)
        {
            return ThisColor;
        }

        public override Vector3 VertextShader(Vector3 vertex_position_WorldSpace, Vector3 vertex_normal_WorldSpace, Vector3 objectposition_WorldSpace)
        {
            return vertex_position_WorldSpace;
        }
    }
}
