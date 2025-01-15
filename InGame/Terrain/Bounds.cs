using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Maths;
using System.Text;
using System.Threading.Tasks;

namespace Athena.InGame.Terrain
{
    /// <summary>
    /// 직육면체 경계 상자.
    /// </summary>
    public class Bounds
    {
        public Vector3 Center;
        public Vector3 Size;

        public Bounds(Vector3 center, Vector3 size)
        {
            Center = center;
            Size = size;
        }

        public Vector3 Min
        {
            get { return Center - Size * 0.5f; }
        }

        public Vector3 Max
        {
            get { return Center + Size * 0.5f; }
        }

        /// <summary>
        /// 바운드와 지점 사이의 최소 거리의 제곱을 계산합니다.
        /// </summary>
        public float SqrDistance(Vector3 point)
        {
            float sqrDistance = 0f;

            Vector3 min = Min;
            Vector3 max = Max;

            // Iterate over each axis (x, y, z)
            for (int i = 0; i < 3; i++)
            {
                float v = point.Get(i);

                if (v < min.Get(i))
                {
                    float delta = min.Get(i) - v;
                    sqrDistance += delta * delta;
                }
                else if (v > max.Get(i))
                {
                    float delta = v - max.Get(i);
                    sqrDistance += delta * delta;
                }
            }

            return sqrDistance;
        }

        public override string ToString()
        {
            return $"Bounds(Center: {Center}, Size: {Size})";
        }
    }
}
