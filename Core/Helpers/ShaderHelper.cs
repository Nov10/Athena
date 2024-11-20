using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renderer;
using Renderer.Maths;
using Renderer.Renderer;
using NPhotoshop;
using NPhotoshop.Core;
using NPhotoshop.Core.Image;

namespace Renderer.Core
{
    internal class ShaderHelper
    {
        public static float CelShading(int level, float ndl, float min, float max)
        {
            //등차수열 이용

            float diff = (max - min) / level;
            float diff_ndl = 2.0f / level;

            //+1 : 평행 이동하여 범위를 조정
            int c = (int)MathF.Abs( MathF.Floor((ndl+1) / diff_ndl));

            return diff * (c) + min;
        }
        public static Color SampleTexture(NBitmap texture, Vector2 uv)
        {
            uv.x = uv.x % 1.0f;
            uv.y = uv.y % 1.0f;
            if (uv.x < 0) uv.x += 1.0f;
            if (uv.y < 0) uv.y += 1.0f;

            int texX = (int)(uv.x * texture.Width);
            int texY = (int)(uv.y * texture.Height);

            texX = Math.Clamp(texX, 0, texture.Width - 1);
            texY = Math.Clamp(texY, 0, texture.Height - 1);

            Color texColor = texture.GetPixel(texX, texY);
            return texColor;
        }
    }
}
