using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Athena.Engine.Core.Image
{
    public class NBitmap
    {
        public WriteableBitmap ConvertToBitmap()
        {
            if (Width * Height == 0)
                return null;

            WriteableBitmap b = new WriteableBitmap(Width, Height);
            using (var stream = b.PixelBuffer.AsStream())
            {
                //BGRA 순서
                byte[] pixelData = new byte[Width * Height * 4];

                //for (int i = 0; i < Pixels.Length; i++)
                System.Threading.Tasks.Parallel.For(0, Pixels.Length, i =>
                {
                    pixelData[i * 4] = Pixels[i].B; // Blue
                    pixelData[i * 4 + 1] = Pixels[i].G; // Green
                    pixelData[i * 4 + 2] = Pixels[i].R; // Red
                    pixelData[i * 4 + 3] = Pixels[i].A; // Alpha
                });

                stream.Write(pixelData, 0, pixelData.Length);
            }
            return b;
        }
        public void ConvertFromBitmap(byte[] pixelData, int width, int height)
        {
            Pixels = new Color[width * height];
            //using (var stream = map.PixelBuffer.AsStream())
            {
                // PixelBuffer에서 데이터를 가져옵니다.

                //for (int i = 0; i < Pixels.Length; i++)
                System.Threading.Tasks.Parallel.For(0, Pixels.Length, i =>
                {
                    Pixels[i].B = pixelData[4 * i]; // Blue
                    Pixels[i].G = pixelData[4 * i + 1]; // Green
                    Pixels[i].R = pixelData[4 * i + 2]; // Red
                    Pixels[i].A = pixelData[4 * i + 3]; // Alpha
                });
            }
        }
        public void ConvertFromBitmap(WriteableBitmap map)
        {
            Pixels = new Color[map.PixelWidth * map.PixelHeight];

            //using (var stream = map.PixelBuffer.AsStream())
            {
                // PixelBuffer에서 데이터를 가져옵니다.
                byte[] pixelData = map.PixelBuffer.ToArray();

                //for (int i = 0; i < Pixels.Length; i++)
                System.Threading.Tasks.Parallel.For(0, Pixels.Length, i =>
                {
                    Pixels[i].B = pixelData[4 * i]; // Blue
                    Pixels[i].R = pixelData[4 * i + 1]; // Green
                    Pixels[i].G = pixelData[4 * i + 2]; // Red
                    Pixels[i].A = pixelData[4 * i + 3]; // Alpha
                });
            }
        }
        public void Clear()
        {
            Clear(new Color(0, 0, 0, 0));
        }
        public virtual void Clear(Color color)
        {
            LoopForPixel((x, y) =>
            {
                SetPixel(x, y, color);
            });
        }
        public void ClearBlack()
        {
            Clear(new Color(0, 0, 0, 255));
        }
        public static NBitmap CreateWhiteNBitmap(int width, int height)
        {
            NBitmap map = new NBitmap(width, height);
            map.LoopForPixel((x, y) =>
            {
                map.SetPixel(x, y, new Color(255, 255, 255, 255));
            });
            return map;
        }

        public Color[] Pixels;
        public int Width;
        public int Height;
        public int Size
        {
            get { return Width * Height; }
        }

        public NBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new Color[Width * Height];
        }
        public void SetPixelsForce(Color[] pixels, int width, int height)
        {
            Pixels = (Color[])pixels.Clone();
            Width = width;
            Height = height;
        }
        public void SetPixels(Color[] pixels)
        {
            if (pixels.Length != Pixels.Length)
                return;
            Pixels = (Color[])pixels.Clone();
        }
        public void SetPixel(int x, int y, Color c)
        {
            if (x + (Height - y - 1) * Width < 0)
                return;
            if (x + (Height - y - 1) * Width >= Pixels.Length)
                return;
            Pixels[x + (Height - y - 1) * Width] = c;
        }
        public Color GetPixel(int x, int y)
        {
            //if (x + y * Width < 0)
            //    return new Color(0, 0, 0, 0);
            //if (x + y * Width >= Pixels.Length)
            //    return new Color(0, 0, 0, 0);
            return Pixels[x + (Height - y - 1) * Width];
        }

        public void LoopForPixel(Action<int, int> loop)
        {
            System.Threading.Tasks.Parallel.For(0, Width * Height, (i) =>
            {
                int y = i / Width;
                int x = i - y * Width;

                loop(x, y);
            });
        }
    }
}