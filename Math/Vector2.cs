using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Maths
{
    public struct Vector2
    {
        public float x;
        public float y;

        public static Vector2 zero
        {
            get { return new Vector2(0, 0); }
        }
        public static Vector2 one
        {
            get { return new Vector2(1, 1); }
        }
        public Vector2()
        {
            x = 0;
            y = 0;
        }
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public readonly float magnitude
        {
            get { return (float)System.MathF.Sqrt(x * x + y * y); }
        }
        public readonly float sqrMagnitude
        {
            get { return x * x + y * y; }
        }
        public readonly Vector2 normalized
        {
            get { return this / magnitude; }
        }

        /// <summary>
        /// 벡터의 원소를 가져옵니다.
        /// <para>i == 0 : x</para>
        /// <para>i == 1 : y</para>
        /// </summary>
        public readonly float Get(int i)
        {
            if (i == 0) return x;
            return y;
        }

        #region Operator
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }
        public static Vector2 operator *(Vector2 a, float scalar)
        {
            return new Vector2(a.x * scalar, a.y * scalar);
        }
        public static Vector2 operator *(float scalar, Vector2 a)
        {
            return  a * scalar;
        }
        public static Vector2 operator /(Vector2 a, float scalar)
        {
            if (scalar == 0)
            {
                return new Vector2(0, 0);
                //throw new DivideByZeroException("Division by zero");
            }

            return new Vector2(a.x / scalar, a.y / scalar);
        }
        #endregion

        #region Math Functions
        public static float Dot(Vector2 a, Vector2 b)
        {
            return (a.x * b.x + a.y * b.y);
        }
        #endregion

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}
