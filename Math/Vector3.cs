using System.Diagnostics.CodeAnalysis;

namespace Athena.Maths
{
    public struct Vector3
    {
        public float x; public float y; public float z;

        public static Vector3 zero
        {
            get { return new Vector3(0, 0, 0); }
        }
        public static Vector3 one
        {
            get { return new Vector3(1, 1, 1); }
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

        public readonly float magnitude
        {
            get { return System.MathF.Sqrt(sqrMagnitude); }
        }

        public readonly float sqrMagnitude
        {
            get { return x * x + y * y + z * z; }
        }

        public readonly Vector3 normalized
        {
            get { return this / magnitude; }
        }

        /// <summary>
        /// 벡터의 원소를 가져옵니다.
        /// i == 0 : x,
        /// i == 1 : y,
        /// i == 2 : z
        /// </summary>
        public readonly float Get(int i)
        {
            if (i == 0) return x;
            if (i == 1) return y;
            return z;
        }

        #region Operator

        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            return new Vector3(left.x + right.x, left.y + right.y, left.z + right.z);
        }
        public static Vector3 operator *(Vector3 left, float right)
        {
            return new Vector3(left.x * right, left.y * right, left.z * right);
        }
        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.x, -v.y, -v.z);
        }
        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return new Vector3(left.x - right.x, left.y - right.y, left.z - right.z);
        }
        public static Vector3 operator /(Vector3 left, float right)
        {
            return left * (1.0f / right);
        }
        public static Vector3 operator *(float left, Vector3 right)
        {
            return right * left;
        }
        public static bool operator ==(Vector3 left, Vector3 right)
        {
            if (left.x != right.x)
                return false;
            if (left.y != right.y)
                return false;
            if (left.z != right.z)
                return false;
            return true;
        }
        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !(left == right);
        }

        #endregion

        #region Math Functions
        public static float Angle(Vector3 a, Vector3 b)
        {
            a = a.normalized;
            b = b.normalized;

            // 내적 (Dot Product)을 이용한 각도 계산 (0 ~ π)
            float angle = System.MathF.Acos(Vector3.Dot(a, b));

            //방향 계산
            Vector3 cross = Vector3.Cross(a, b);
            float sign = Vector3.Dot(cross, new Vector3(0, 1, 0));

            // 방향에 따라 각도를 조정
            if (sign < 0)
            {
                angle = 2 * (float)System.MathF.PI - angle;
            }

            return angle * XMath.Rad2Deg;
        }
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            if (t > 1) return b;
            if (t < 0) return a;
            return (1 - t) * a + t * b;
        }
        public static Vector3 ElementDivide(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
        }
        public static Vector3 ElementProduct(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
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
        /// <summary>
        /// (0, 1, 0)과의 외적을 계산합니다. 순서는  Cross(new Vector3(0, 1, 0), v)와 동일함.
        /// </summary>
        public static Vector3 Cross_withYAxis(Vector3 v)
        {
            return new Vector3(
                v.z,
                0,
                - v.x
                );
        }
        /// <summary>
        /// 두 벡터의 외적의 Z축 성분을 계산합니다.  v1.x * v2.y - v1.y * v2.x와 동일함.
        /// </summary>
        public static float Cross_Z(Vector3 v1, Vector3 v2)
        {
            return v1.x * v2.y - v1.y * v2.x;
        }

        #endregion

        #region Overridings

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
