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

namespace Athena.Engine.Core.Rendering.Shaders
{
    public class NormalShader : CustomShader
    {
        public NBitmap MainTexture;
        public NBitmap NormalTexture;

        public NormalShader()
        {
            MainTexture = new NBitmap(1, 1);
            NormalTexture = new NBitmap(1, 1);
        }

        public override Color FragmentShader(Raster raster, Vector3 light)
        {
            //var normal = (ShaderHelper.SampleTexture(NormalTexture, raster.UV).GetAsVector3() / 255.0f).normalized;
            var normal = raster.Normal_WorldSpace;
            float brightness = Vector3.Dot(normal, light) * 0.5f + 0.5f;
            var c2 = ShaderHelper.SampleTexture(MainTexture, raster.UV) * brightness;

            //raster.Tangent, raster.BitTangent, raster.Normal_WorldSpace
            c2.A = 255;
            return c2;
        }

        public override Vector3 VertextShader(Vector3 vertex_position_WorldSpace, Vector3 vertex_normal_WorldSpace, Vector3 objectposition_WorldSpace)
        {
            return vertex_position_WorldSpace;
        }
    }
}
