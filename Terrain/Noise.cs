using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoiseTest;
using Athena.Maths;

namespace Athena.Terrain
{
    public static class Noise
    {
        public enum eNormalizeMode
        {
            Local,
            Global
        }

        static OpenSimplexNoise N;
        public static float[,] GenerateNoiseMap(int width, int height, float scale, int seed, int octaves, float persistance, float lacunarity, Vector2 offset, eNormalizeMode mode)
        {
            if(N == null)
            {
                N = new OpenSimplexNoise(0);
            }
            float[,] noiseMap = new float[width, height];

            System.Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];

            float maxPossibleHeight = 0;
            float amplitude = 1;
            float frequency = 1;
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= persistance;
            }

            if (scale <= 0)
                scale = 0.0001f;


            float minLocalHeight = float.MaxValue;
            float maxLocalHeight = float.MinValue;

            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;


                    for (int o = 0; o < octaves; o++)
                    {
                        float sampleX = (x - halfWidth + octaveOffsets[o].x) / scale * frequency;
                        float sampleY = (y - halfHeight + octaveOffsets[o].y) / scale * frequency;

                        float perlinValue = Noise2d.Noise(sampleX, sampleY);
                        //float perlinValue = (float)N.Evaluate(x,y) * 0.5f + 0.5f;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }
                    if (noiseHeight > maxLocalHeight)
                    {
                        maxLocalHeight = noiseHeight;
                    }
                    if (noiseHeight < minLocalHeight)
                    {
                        minLocalHeight = noiseHeight;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }
            System.Diagnostics.Debug.WriteLine($"{minLocalHeight} : {maxLocalHeight}");
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (mode == eNormalizeMode.Local)
                        noiseMap[x, y] = XMath.InverseLerp(minLocalHeight, maxLocalHeight, noiseMap[x, y]);
                    else
                    {
                        float normalizedHeight = (noiseMap[x, y] + 0.8f) / (maxPossibleHeight / 1.75f);
                        noiseMap[x, y] = XMath.Clamp(normalizedHeight, 0, int.MaxValue);
                    }
                }
            }

            return noiseMap;
        }
    }
    public static class Noise2d
    {
        private static Random _random = new Random();
        private static int[] _permutation;

        private static Vector2[] _gradients;

        static Noise2d()
        {
            CalculatePermutation(out _permutation);
            CalculateGradients(out _gradients);
        }

        private static void CalculatePermutation(out int[] p)
        {
            p = Enumerable.Range(0, 256).ToArray();

            /// shuffle the array
            for (var i = 0; i < p.Length; i++)
            {
                var source = _random.Next(p.Length);

                var t = p[i];
                p[i] = p[source];
                p[source] = t;
            }
        }

        /// <summary>
        /// generate a new permutation.
        /// </summary>
        public static void Reseed()
        {
            CalculatePermutation(out _permutation);
        }

        private static void CalculateGradients(out Vector2[] grad)
        {
            grad = new Vector2[256];

            for (var i = 0; i < grad.Length; i++)
            {
                Vector2 gradient;

                do
                {
                    gradient = new Vector2((float)(_random.NextDouble() * 2 - 1), (float)(_random.NextDouble() * 2 - 1));
                }
                while (gradient.sqrMagnitude >= 1);

                gradient.Normalize();

                grad[i] = gradient;
            }

        }

        private static float Drop(float t)
        {
            t = Math.Abs(t);
            return 1f - t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static float Q(float u, float v)
        {
            return Drop(u) * Drop(v);
        }

        public static float Noise(float x, float y)
        {
            var cell = new Vector2((float)Math.Floor(x), (float)Math.Floor(y));

            var total = 0f;

            var corners = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

            foreach (var n in corners)
            {
                var ij = cell + n;
                var uv = new Vector2(x - ij.x, y - ij.y);
                int k = (int)ij.x % _permutation.Length;
                if (k < 0)
                    k += _permutation.Length;
                var index = _permutation[k];

                k = (index + (int)ij.y) % _permutation.Length;
                if (k < 0)
                    k += _permutation.Length;
                index = _permutation[k];

                var grad = _gradients[index % _gradients.Length];

                total += Q(uv.x, uv.y) * Vector2.Dot(grad, uv);
            }

            return Math.Max(Math.Min(total, 1f), -1f);
        }

    }
}
