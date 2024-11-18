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
        protected override Color FragmentShader(int screen_x, int screen_y, Vector3 position_ScreenSpace, Vector3 normal, Vector3 lambda, Vector2 uv)
        {
            float brightness = Vector3.Dot(normal.normalized, -(new Vector3(-1f, -1, 0)).normalized);
            brightness = 0.5f * brightness + 0.5f;
            byte intensity = (byte)(brightness * 255);
            var color = new NPhotoshop.Core.Image.Color(intensity, intensity, intensity, 255);
            var c2 =  SampleTexture(uv);
            return c2;
        }
        private void LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Texture file not found: {path}");
            }

            // 이미지 파일 로드
            using (var stream = File.OpenRead(path))
            {
                var decoder = BitmapDecoder.CreateAsync(stream.AsRandomAccessStream()).AsTask().Result;
               var textureBitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);

                var pixelBuffer = decoder.GetPixelDataAsync().AsTask().Result;
                var  pixelData = pixelBuffer.DetachPixelData();
                texture = new NBitmap(textureBitmap.PixelWidth, textureBitmap.PixelHeight);
                texture.ConvertFromBitmap(pixelData, textureBitmap.PixelWidth, textureBitmap.PixelHeight);
            }
        }
        public Shader1()
        {
            texture = new NBitmap(10,10);
            LoadTexture(@"C:\Mando_Helm_Mat_Colour.png");
        }
        NBitmap texture;
        private Color SampleTexture(Vector2 uv)
        {
            // Wrap UV coordinates
            uv.x = uv.x % 1.0f;
            uv.y = uv.y % 1.0f;
            if (uv.x < 0) uv.x += 1.0f;
            if (uv.y < 0) uv.y += 1.0f;

            // Convert UV to texture coordinates
            int texX = (int)(uv.x * texture.Width);
            int texY = (int)(uv.y * texture.Height);

            // Ensure texture coordinates are within bounds
            texX = Math.Clamp(texX, 0, texture.Width - 1);
            texY = Math.Clamp(texY, 0, texture.Height - 1);

            // Get the color from the texture
            Color texColor = texture.GetPixel(texX, texY);
            return texColor;
            //return new Color(texColor.R / 255.0f, texColor.G / 255.0f, texColor.B / 255.0f, texColor.A / 255.0f);
        }

        protected override Vector3 VertextShader(Vector3 vertex_position_WorldSpace, Vector3 vertex_normal_WorldSpace, Vector3 objectposition_WorldSpace)
        {
            return vertex_position_WorldSpace;
        }
    }
}
