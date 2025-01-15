using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Athena.Engine.Core.Image
{
    /// <summary>
    /// Array-Based Bitmap. ���� ��Ʈ���Դϴ�.
    /// </summary>
    public class NBitmap
    {
        public Color[] Pixels { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
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

        #region Convert Bitmap
        /// <summary>
        /// ��Ʈ���� WriteableBitmap���� ��ȯ�մϴ�.
        /// **�� �Լ��� ȯ�濡 �����մϴ�.**
        /// </summary>
        public WriteableBitmap ConvertToBitmap()
        {
            if (Width * Height == 0)
                return null;

            WriteableBitmap b = new WriteableBitmap(Width, Height);
            using (var stream = b.PixelBuffer.AsStream())
            {
                //BGRA ����
                byte[] pixelData = new byte[Width * Height * 4];

                LoopForPixel1D((i) =>
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
        /// <summary>
        /// WriteableBitmap�κ��� ��Ʈ���� �����մϴ�.
        /// **�� �Լ��� ȯ�濡 �����մϴ�.**
        /// </summary>
        public void ConvertFromBitmap(WriteableBitmap map)
        {
            Pixels = new Color[map.PixelWidth * map.PixelHeight];

            byte[] pixelData = map.PixelBuffer.ToArray();

            LoopForPixel1D((i) =>
            {
                Pixels[i] = new Color(pixelData[4 * i + 2], pixelData[4 * i + 1], pixelData[4 * i], pixelData[4 * i + 3]);
            });
        }
        public void ConvertFromByteArray(byte[] pixelData, int width, int height)
        {
            Pixels = new Color[width * height];
            LoopForPixel1D((i) =>
            {
                Pixels[i] = new Color(pixelData[4 * i + 2], pixelData[4 * i + 1], pixelData[4 * i], pixelData[4 * i + 3]);
            });
        }
        #endregion

        #region Utility
        /// <summary>
        /// ��� �ȼ��� ���� �Լ��� �輱�մϴ�.
        /// ���ķ� ó���ǹǷ�, �ǵ��� �� �Լ��� �̿��ϴ� ���� �����ϴ�.
        /// </summary>
        public void LoopForPixel2D(Action<int, int> loop)
        {
            System.Threading.Tasks.Parallel.For(0, Width * Height, (i) =>
            {
                int y = i / Width;
                int x = i - y * Width;

                loop(x, y);
            });
        }
        /// <summary>
        /// ��� �ȼ��� ���� �Լ��� �輱�մϴ�.
        /// ���ķ� ó���ǹǷ�, �ǵ��� �� �Լ��� �̿��ϴ� ���� �����ϴ�.
        /// </summary>
        public void LoopForPixel1D(Action<int> loop)
        {
            System.Threading.Tasks.Parallel.For(0, Width * Height, (i) => {
            
                loop(i);
            });
        }
        /// <summary>
        /// ��� �ȼ��� ������ �������� �ʱ�ȭ�մϴ�.
        /// </summary>
        public virtual void Clear(Color color)
        {
            LoopForPixel2D((x, y) =>
            {
                SetPixel(x, y, color);
            });
        }
        public void Clear()
        {
            Clear(Color.zero);
        }
        public void ClearBlack()
        {
            Clear(Color.black);
        }
        public void ClearWhite()
        {
            Clear(Color.white);
        }
        #endregion

        #region Get/Set Pixel
        /// <summary>  ���� ��Ʈ���� �ȼ��� �����մϴ�. ���� ��Ʈ���� ũ�⿡ �������� �ʽ��ϴ�.  </summary>
        public void SetPixelsForce(Color[] pixels, int width, int height)
        {
            if (width * height != pixels.Length)
                throw new Exception($"Length of Pixels mismatch widh and height. Pixel Count : {pixels.Length}, Width*Height : {width * height}");
            Pixels = (Color[])pixels.Clone();
            Width = width;
            Height = height;
        }
        /// <summary>  ���� ��Ʈ���� �ȼ��� �����մϴ�. �ȼ��� ������ �����ؾ� �����˴ϴ�.  </summary>
        public void SetPixels(Color[] pixels)
        {
            if (pixels.Length != Pixels.Length)
                return;
            Pixels = (Color[])pixels.Clone();
        }
        public void SetPixel(int x, int y, Color c)
        {
            //if (x + (Height - y - 1) * Width < 0)
            //    return;
            //if (x + (Height - y - 1) * Width >= Pixels.Length)
            //    return;
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
        #endregion

    }
}