using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Maths
{
    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public static Vector4 zero
        {
            get { return new Vector4(0, 0, 0, 0); }
        }
        public static Vector4 one
        {
            get { return new Vector4(1, 1, 1, 1); }
        }
        public Vector4()
        {
            x = 0; y = 0; z = 0; w = 0;
        }
        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public readonly float magnitude
        {
            get { return (float)System.MathF.Sqrt(sqrMagnitude); }
        }
        public readonly float sqrMagnitude
        {
            get { return x * x + y * y + z * z + w * w; }
        }
        public readonly Vector4 normalized
        {
            get { return this / magnitude; }
        }

        /// <summary>
        /// 벡터의 원소를 가져옵니다.
        /// <para>i == 0 : x</para>
        /// <para>i == 1 : y</para>
        /// <para>i == 2 : z</para>
        /// <para>i == 3 : w</para>
        /// </summary>
        public readonly float Get(int i)
        {
            if (i == 0) return x;
            if (i == 1) return y;
            if (i == 2) return z;
            return w;
        }

        #region  Operator
        public static Vector4 operator +(Vector4 v1, Vector4 v2)
        {
            return new Vector4(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
        }
        public static Vector4 operator -(Vector4 v1, Vector4 v2)
        {
            return new Vector4(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
        }
        public static Vector4 operator *(float scalar, Vector4 v)
        {
            return new Vector4(scalar * v.x, scalar * v.y, scalar * v.z, scalar * v.w);
        }
        public static Vector4 operator *(Vector4 v, float scalar)
        {
            return scalar * v;
        }
        public static Vector4 operator /(Vector4 v, float scalar)
        {
            if (scalar == 0)
            {
                return new Vector4(0, 0, 0, 0);
            }
            return new Vector4(v.x / scalar, v.y / scalar, v.z / scalar, v.w / scalar);
        }
        #endregion

        #region Math Functions
        public static float Dot(Vector4 v1, Vector4 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z + v1.w * v2.w;
        }
        #endregion
        public override string ToString()
        {
            return $"({x}, {y}, {z}, {w})";
        }
    }
}
