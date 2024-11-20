using Assimp.Unmanaged;
using Microsoft.UI.Xaml.Media.Imaging;
using NPhotoshop.Core.Image;
using Renderer.Core.Shader;
using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Renderer.Core.PBR
{
    public class Shader1 : CustomShader
    {
        public NBitmap MainTexture;
        public Shader1()
        {
            MainTexture = new NBitmap(1,1);
        }

        protected override Color FragmentShader(int screen_x, int screen_y, Vector3 position_ScreenSpace, Vector3 normal_WorldSpace, Vector3 lambda, Vector2 uv)
        {
            float brightness = Vector3.Dot(normal_WorldSpace.normalized, -(new Vector3(-1f, -1, 0)).normalized);
            //brightness = 0.5f * brightness + 0.5f;
            //byte intensity = (byte)(brightness * 255);
            var c2 = ShaderHelper.SampleTexture(MainTexture, uv) * ShaderHelper.CelShading(3, brightness, 0.1f, 1);
            c2.A = 255;
            return c2;
        }

        protected override Vector3 VertextShader(Vector3 vertex_position_WorldSpace, Vector3 vertex_normal_WorldSpace, Vector3 objectposition_WorldSpace)
        {
            return vertex_position_WorldSpace;
        }
    }
}
