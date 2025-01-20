using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Engine.Core.Image
{
    public class NBitmapFloat
    {
        public float[] Pixels { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Size
        {
            get { return Width * Height; }
        }
        public NBitmapFloat(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new float[Width * Height];
        }

        #region Convert Bitmap
        /// <summary>
        /// 비트맵을 WriteableBitmap으로 변환합니다.
        /// **이 함수는 환경에 의존합니다.**
        /// </summary>
        public WriteableBitmap ConvertToBitmap()
        {
            if (Width * Height == 0)
                return null;

            WriteableBitmap b = new WriteableBitmap(Width, Height);
            using (var stream = b.PixelBuffer.AsStream())
            {
                //BGRA 순서
                byte[] pixelData = new byte[Width * Height * 4];

                LoopForPixel1D((i) =>
                {
                    pixelData[i * 4] = (byte)(Pixels[i] * 255); // Blue
                    pixelData[i * 4 + 1] = (byte)(Pixels[i] * 255); // Green
                    pixelData[i * 4 + 2] = (byte)(Pixels[i] * 255); // Red
                    pixelData[i * 4 + 3] = 255; // Alpha
                });

                stream.Write(pixelData, 0, pixelData.Length);
            }
            return b;
        }
        /// <summary>
        /// WriteableBitmap로부터 비트맵을 생성합니다.
        /// **이 함수는 환경에 의존합니다.**
        /// </summary>
        public void ConvertFromBitmap(WriteableBitmap map)
        {
            Pixels = new float[map.PixelWidth * map.PixelHeight];

            byte[] pixelData = map.PixelBuffer.ToArray();

            LoopForPixel1D((i) =>
            {
                Pixels[i] = (pixelData[4 * i + 2]+ pixelData[4 * i + 1]+ pixelData[4 * i]+ pixelData[4 * i + 3]) / 255f / 4f;
            });
        }
        #endregion

        #region Utility
        /// <summary>
        /// 모든 픽셀에 대해 함수를 계선합니다.
        /// 병렬로 처리되므로, 되도록 이 함수를 이용하는 것이 좋습니다.
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
        /// 모든 픽셀에 대해 함수를 계선합니다.
        /// 병렬로 처리되므로, 되도록 이 함수를 이용하는 것이 좋습니다.
        /// </summary>
        public void LoopForPixel1D(Action<int> loop)
        {
            System.Threading.Tasks.Parallel.For(0, Width * Height, (i) => {

                loop(i);
            });
        }
        /// <summary>
        /// 모든 픽셀을 지정된 색상으로 초기화합니다.
        /// </summary>
        public virtual void Clear(float color)
        {
            LoopForPixel2D((x, y) =>
            {
                SetPixel(x, y, color);
            });
        }
        public void Clear()
        {
            Clear(0);
        }
        public void ClearOne()
        {
            Clear(1);
        }
        #endregion

        #region Get/Set Pixel
        /// <summary>  현재 비트맵의 픽셀을 설정합니다. 현재 비트맵의 크기에 의존하지 않습니다.  </summary>
        public void SetPixelsForce(float[] pixels, int width, int height)
        {
            if (width * height != pixels.Length)
                throw new Exception($"Length of Pixels mismatch widh and height. Pixel Count : {pixels.Length}, Width*Height : {width * height}");
            Pixels = (float[])pixels.Clone();
            Width = width;
            Height = height;
        }
        /// <summary>  현재 비트맵의 픽셀을 설정합니다. 픽셀의 개수가 동일해야 설정됩니다.  </summary>
        public void SetPixels(float[] pixels)
        {
            if (pixels.Length != Pixels.Length)
                return;
            Pixels = (float[])pixels.Clone();
        }
        public void SetPixel(int x, int y, float c)
        {
            //if (x + (Height - y - 1) * Width < 0)
            //    return;
            //if (x + (Height - y - 1) * Width >= Pixels.Length)
            //    return;
            Pixels[x + (Height - y - 1) * Width] = c;
        }
        public float GetPixel(int x, int y)
        {
            //if (x + y * Width < 0)
            //    return new float(0, 0, 0, 0);
            //if (x + y * Width >= Pixels.Length)
            //    return new float(0, 0, 0, 0);
            return Pixels[x + (Height - y - 1) * Width];
        }
        #endregion
    }
}
