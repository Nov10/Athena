using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Maths
{
    public struct Vector4
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }
        public Vector4 Normalize()
        {
            return this / Magnitude();
        }
        public float Magnitude()
        {
            return MathF.Sqrt(x * x + y * y + z * z + w * w);
        }
        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        // 벡터 덧셈
        public static Vector4 operator +(Vector4 v1, Vector4 v2)
        {
            return new Vector4(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
        }

        // 벡터 뺄셈
        public static Vector4 operator -(Vector4 v1, Vector4 v2)
        {
            return new Vector4(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
        }

        // 실수배
        public static Vector4 operator *(float scalar, Vector4 v)
        {
            return new Vector4(scalar * v.x, scalar * v.y, scalar * v.z, scalar * v.w);
        }
        public static Vector4 operator *(Vector4 v, float scalar)
        {
            return scalar * v;
        }
        // 나누기
        public static Vector4 operator /(Vector4 v, float scalar)
        {
            if (scalar == 0)
            {
                return new Vector4(0, 0, 0, 0);
            }
            return new Vector4(v.x / scalar, v.y / scalar, v.z / scalar, v.w / scalar);
        }

        // 내적 (Dot Product)
        public static float Dot(Vector4 v1, Vector4 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z + v1.w * v2.w;
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z}, {w})";
        }
    }
}
