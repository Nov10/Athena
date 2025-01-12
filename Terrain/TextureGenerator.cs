using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Engine.Core.Image;

namespace Athena.Terrain
{

    public static class TextureGenerator
    {
        public static NBitmap TextureFromColorMap(Color[] color, int width, int height)
        {
            NBitmap tex = new NBitmap(width, height);
            //tex.filterMode = FilterMode.Point;
            //tex.wrapMode = TextureWrapMode.Clamp;
            tex.SetPixels(color);
            //tex.Apply();
            return tex;
        }

        public static NBitmap TextureFromHeightMap(float[,] heightMap)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            NBitmap texture = new NBitmap(width, height);

            Color[] colorMap = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
                }
            }


            return TextureFromColorMap(colorMap, width, height);
        }
    }
}
