using Assimp;
using Athena.Engine.Core.Image;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Engine.Core.Image
{
    /// <summary>
    /// Bitmap for Rendering. Depth가 존재합니다.
    /// </summary>
    public class RenderBitmap
    {
        private NBitmap Bitmap;
        public int NowDepth { get; private set; }

        public int Width { get { return Bitmap.Width; } }
        public int Height { get { return Bitmap.Height; } }

        public RenderBitmap(int width, int height)
        {
            NowDepth = 0;
            Bitmap = new NBitmap(width, height);
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
        public void SetPixels(Color[] pixels, int depth)
        {
            if (CompareDepth(depth) == false)
                return;
            Bitmap.SetPixels(pixels);
        }
        public Color[] GetPixels()
        {
            return Bitmap.Pixels;
        }
        #endregion

        #region Utility
        public void Clear(Color color)
        {
            NowDepth = 0;
            Bitmap.Clear(color);
        }
        public void Clear()
        {
            Clear(Color.zero);
        }
        public void ClearBlack()
        {
            Clear(Color.black);
        }
        #endregion

    }
}
