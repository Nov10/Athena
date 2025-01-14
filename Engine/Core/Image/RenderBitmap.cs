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
    public class RenderBitmap
    {
        private NBitmap Bitmap;
        private int NowDepth;
        public WriteableBitmap ConvertToBitmap()
        {
            return Bitmap.ConvertToBitmap();
        }

        public RenderBitmap(int width, int height)
        {
            NowDepth = 0;
            Bitmap = new NBitmap(width, height);
        }
        public void SetPixels(Color[] pixels)
        {
            Bitmap.SetPixels(pixels);
        }
        public Color[] GetPixels()
        {
            return Bitmap.Pixels;
        }
        public void Clear()
        {
            Clear(new Color(0, 0, 0, 0));
        }
        public virtual void Clear(Color color)
        {
            NowDepth = 0;
            Bitmap.LoopForPixel((x, y) =>
            {
                Bitmap.SetPixel(x, y, color);
            });
        }
        public void ClearBlack()
        {
            Clear(new Color(0, 0, 0, 255));
        }

        public bool CompareDepth(int depth)
        {
            if (depth >= NowDepth)
                return true;
            return false;
        }
    }
}
