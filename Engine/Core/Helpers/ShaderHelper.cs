using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using Athena.Engine.Core.Image;
using ILGPU;
using Athena.Engine.Core.Rendering;

namespace Athena.Engine.Helpers
{
    public class ShaderHelper
    {
        public static float CelShading(int level, float ndl, float min, float max)
        {
            //등차수열 이용

            float diff = (max - min) / level;
            float diff_ndl = 2.0f / level;

            //+1 : 평행 이동하여 범위를 조정
            int c = (int)MathF.Abs(MathF.Floor((ndl + 1) / diff_ndl));

            return diff * c + min;
        }
        public static float Mod(float x, float y)
        {
            return x - ILGPU.Algorithms.XMath.Floor(x / y) * y;
        }
        public static float ModOne(float x)
        {
            return x - ILGPU.Algorithms.XMath.Floor(x);
        }
        public static float Clamp01(float x)
        {
            return ILGPU.IntrinsicMath.Clamp(x, 0, 1);
        }
        public static int CalculateFrameBufferIndexOfRaster(Raster raster, int width)
        {
            return raster.x + raster.y * width;
        }

        public static Color SampleTexture_UVMod_GPU(Texture2D texture, Vector2 uv)
        {
            //ILGPU.Algorithms.XMath.Rem은 컴파일 에러 뜸. 뭐임?
            return texture.GetPixel(
                ILGPU.IntrinsicMath.Clamp((int)(ModOne(uv.x) * texture.Width), 0, texture.Width - 1), 
                ILGPU.IntrinsicMath.Clamp((int)(ModOne(uv.y) * texture.Height), 0, texture.Height - 1));
        }
        public static float SampleTexture_UVMod_GPU(Texture2DFloat texture, Vector2 uv)
        {
            //ILGPU.Algorithms.XMath.Rem은 컴파일 에러 뜸. 뭐임?
            return texture.GetPixel(
                ILGPU.IntrinsicMath.Clamp((int)(ModOne(uv.x) * texture.Width), 0, texture.Width - 1),
                ILGPU.IntrinsicMath.Clamp((int)(ModOne(uv.y) * texture.Height), 0, texture.Height - 1));
        }
        public static float SampleTexture_UVClamp_GPU(Texture2DFloat texture, Vector2 uv)
        {
            //ILGPU.Algorithms.XMath.Rem은 컴파일 에러 뜸. 뭐임?
            return texture.GetPixel(
                ILGPU.IntrinsicMath.Clamp((int)(Clamp01(uv.x) * texture.Width), 0, texture.Width - 1),
                ILGPU.IntrinsicMath.Clamp((int)(Clamp01(uv.y) * texture.Height), 0, texture.Height - 1));
        }
        public static Color SampleTexture(Texture2D texture, Vector2 uv)
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
