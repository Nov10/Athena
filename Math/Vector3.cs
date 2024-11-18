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
