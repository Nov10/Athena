using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Maths
{
    public struct Vector3
    {
        public float x; public float y; public float z;

        public static Vector3 zero
        {
            get { return new Vector3(0, 0, 0); }
        }
        public Vector3 normalized
        {
            get { return this / magnitude; }
        }
        public Vector3()
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;
        }
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 operator /(Vector3 left, float right)
        {
            return new Vector3(left.x / right, left.y / right, left.z / right);
        }
        public static Vector3 operator *(Vector3 left, float right)
        {
            return new Vector3(left.x * right, left.y * right, left.z * right);
        }
        public static Vector3 operator *(float left, Vector3 right)
        {
            return right * left;
        }
        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            return new Vector3(left.x + right.x, left.y + right.y, left.z + right.z);
        }
        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return new Vector3(left.x - right.x, left.y - right.y, left.z - right.z);
        }
        public static Vector3 operator -(Vector3 v)
        {
            return v * -1;
        }

        public float magnitude
        {
            get { return (float)System.Math.Sqrt(sqrMagnitude); }
        }

        public float sqrMagnitude
        {
            get { return x * x + y * y + z * z; }
        }

        public static float Angle(Vector3 a, Vector3 b)
        {
            // 두 벡터를 정규화 (Normalize)
            a = a.normalized;
            b = b.normalized;

            // 내적 (Dot Product)을 이용한 각도 계산 (0 ~ π)
            float angle = MathF.Acos(Vector3.Dot(a, b));

            // 외적 (Cross Product)을 이용하여 방향 결정
            Vector3 cross = Vector3.Cross(a, b);
            float sign = Vector3.Dot(cross, new Vector3(0, 1, 0));

            // 방향에 따라 각도를 조정
            if (sign < 0)
            {
                angle = 2 * (float)Math.PI - angle;
            }

            // 라디안을 도(degree)로 변환
            return angle;
        }
        public static float Dot(Vector3 v1, Vector3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }
        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            return new Vector3(
                v1.y * v2.z - v1.z * v2.y,
                v1.z * v2.x - v1.x * v2.z,
                v1.x * v2.y - v1.y * v2.x
                );
        }
        public static float Cross_Z(Vector3 v1, Vector3 v2)
        {
            return v1.x * v2.y - v1.y * v2.x;
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
}
