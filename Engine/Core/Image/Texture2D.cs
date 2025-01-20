using Assimp;
using Athena.Engine.Core.Rendering;
using Athena.Engine.Core.Rendering.Shaders;
using ILGPU;
using ILGPU.Runtime;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Engine.Core.Image
{
    public class Texture2DHelper
    {

        public static void ConvertFromBitmap(Texture2DWrapper texture, NBitmap map)
        {
            texture.Texture = new Texture2D(map.Width, map.Height);
            Color[] c = new Color[texture.Texture.Width * texture.Texture.Height];
            texture.Buffer = GPUAccelator.Accelerator.Allocate1D<Color>(map.Width * map.Height);
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    c[x + (map.Height - y - 1) * map.Width] = (map.GetPixel(x, y));
                }
            }
            texture.Buffer.CopyFromCPU(c);
            texture.Texture.Pixels = texture.Buffer.View;
            texture.Apply();
        }
        public static void ConvertFromBitmap(Texture2DFloatWrapper texture, NBitmapFloat map)
        {
            texture.Texture = new Texture2DFloat(map.Width, map.Height);
            float[] c = new float[texture.Texture.Width * texture.Texture.Height];
            texture.Buffer = GPUAccelator.Accelerator.Allocate1D<float>(map.Width * map.Height);
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    c[x + (map.Height - y - 1) * map.Width] = map.GetPixel(x, y);
                }
            }
            texture.Buffer.CopyFromCPU(c);
            texture.Texture.Pixels = texture.Buffer.View;
            texture.Apply();
        }
        public static void ConvertFromBitmap(ref Texture2D texture, NBitmap map)
        {
            texture = new Texture2D(map.Width, map.Height);
            Color[] c = new Color[texture.Width * texture.Height];
            MemoryBuffer1D<Color, Stride1D.Dense> buffer = GPUAccelator.Accelerator.Allocate1D<Color>(map.Width * map.Height);
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    c[x + (map.Height - y - 1) * map.Width] = (map.GetPixel(x, y));
                }
            }
            buffer.CopyFromCPU(c);
            texture.Pixels = buffer.View;
        }
    }
    public class Texture2DWrapper
    {
        public Texture2D Texture;
        public MemoryBuffer1D<Color, Stride1D.Dense> Buffer;
        public System.Action Apply;
    }
    public struct Texture2D
    {
        public ArrayView<Color> Pixels;
        public int Width;
        public int Height;
        public Texture2D(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new ArrayView<Color>();
        }

        public Color GetPixel(int x, int y)
        {
            return Pixels[x + (Height - y - 1) * Width];
        }
    }

    public class Texture2DFloatWrapper
    {
        public Texture2DFloat Texture;
        public MemoryBuffer1D<float, Stride1D.Dense> Buffer;
        public System.Action Apply;
    }
    public struct Texture2DFloat
    {
        public ArrayView<float> Pixels;
        public int Width;
        public int Height;
        public Texture2DFloat(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new ArrayView<float>();
        }

        public float GetPixel(int x, int y)
        {
            return Pixels[x + y * Width];
        }
    }
}
