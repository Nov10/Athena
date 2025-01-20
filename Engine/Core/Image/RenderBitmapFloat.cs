using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Engine.Core.Image
{
    public class RenderBitmapFloat
    {
        private NBitmapFloat Bitmap;
        public int NowDepth { get; private set; }

        public int Width { get { return Bitmap.Width; } }
        public int Height { get { return Bitmap.Height; } }

        public RenderBitmapFloat(int width, int height)
        {
            NowDepth = 0;
            Bitmap = new NBitmapFloat(width, height);
        }
        public NBitmapFloat GetAsNBitmapFloat()
        {
            return Bitmap;
        }
        public bool CompareDepth(int depth)
        {
            if (depth >= NowDepth)
                return true;
            return false;
        }
        #region Convert Bitmap
        /// <summary>
        /// 비트맵을 WriteableBitmap으로 변환합니다.
        /// **이 함수는 환경에 의존합니다.**
        /// </summary>
        public WriteableBitmap ConvertToBitmap()
        {
            return Bitmap.ConvertToBitmap();
        }
        #endregion

        #region Get/Set Pixel
        public void SetPixels(float[] pixels, int depth)
        {
            if (CompareDepth(depth) == false)
                return;
            Bitmap.SetPixels(pixels);
        }
        public float[] GetPixels()
        {
            return Bitmap.Pixels;
        }
        public float GetPixel(int x, int y)
        {
            return Bitmap.GetPixel(x, y);
        }
        #endregion

        #region Utility
        public void Clear(float color)
        {
            NowDepth = 0;
            Bitmap.Clear(color);
        }
        public void Clear()
        {
            Clear(0);
        }
        #endregion
    }
}
