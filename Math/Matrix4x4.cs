﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace Athena.Maths
{
    public struct Matrix4x4
    {
        public float
        e11, e12, e13, e14,
        e21, e22, e23, e24,
        e31, e32, e33, e34,
        e41, e42, e43, e44;

        public override string ToString()
        {
            return $"{e11}, {e12}, {e13}, {e14}\n{e21}, {e22}, {e23}, {e24}\n{e31}, {e32}, {e33}, {e34}\n{e41}, {e42}, {e43}, {e44}";
        }

        public Matrix4x4(
            float e11, float e12, float e13, float e14,
            float e21, float e22, float e23, float e24,
            float e31, float e32, float e33, float e34,
            float e41, float e42, float e43, float e44)
        {
            this.e11 = e11; this.e12 = e12; this.e13 = e13; this.e14 = e14;
            this.e21 = e21; this.e22 = e22; this.e23 = e23; this.e24 = e24;
            this.e31 = e31; this.e32 = e32; this.e33 = e33; this.e34 = e34;
            this.e41 = e41; this.e42 = e42; this.e43 = e43; this.e44 = e44;
        }

        public static bool operator ==(Matrix4x4 a, Matrix4x4 b)
        {
            if (a.e11 != b.e11)
                return false;
            if(a.e12 != b.e12)
                return false;
            if (a.e13 != b.e13)
                return false;
            if (a.e14 != b.e14)
                return false;
            if (a.e21 != b.e21)
                return false;
            if (a.e22 != b.e22)
                return false;
            if (a.e23 != b.e23)
                return false;
            if (a.e24 != b.e24)
                return false;
            if (a.e31 != b.e31)
                return false;
            if (a.e32 != b.e32)
                return false;
            if (a.e33 != b.e33)
                return false;
            if (a.e34 != b.e34)
                return false;
            if (a.e41 != b.e41)
                return false;
            if (a.e42 != b.e42)
                return false;
            if (a.e43 != b.e43)
                return false;
            if (a.e44 != b.e44)
                return false;
            return true;
        }
        public static bool operator !=(Matrix4x4 a, Matrix4x4 b)
        {
            return !(a == b);
        }
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static Matrix4x4 operator -(Matrix4x4 a)
        {
            return new Matrix4x4(
                -a.e11, -a.e12, -a.e13, -a.e14,
                -a.e21, -a.e22, -a.e23, -a.e24,
                -a.e31, -a.e32, -a.e33, -a.e34,
                -a.e41, -a.e42, -a.e43, -a.e44);
        }
        public static Matrix4x4 operator +(Matrix4x4 a, Matrix4x4 b)
        {
            return new Matrix4x4(
                a.e11 + b.e11, a.e12 + b.e12, a.e13 + b.e13, a.e14 + b.e14,
                a.e21 + b.e21, a.e22 + b.e22, a.e23 + b.e23, a.e24 + b.e24,
                a.e31 + b.e31, a.e32 + b.e32, a.e33 + b.e33, a.e34 + b.e34,
                a.e41 + b.e41, a.e42 + b.e42, a.e43 + b.e43, a.e44 + b.e44);
        }
        public static Matrix4x4 operator -(Matrix4x4 a, Matrix4x4 b)
        {
            return new Matrix4x4(
                a.e11 - b.e11, a.e12 - b.e12, a.e13 - b.e13, a.e14 - b.e14,
                a.e21 - b.e21, a.e22 - b.e22, a.e23 - b.e23, a.e24 - b.e24,
                a.e31 - b.e31, a.e32 - b.e32, a.e33 - b.e33, a.e34 - b.e34,
                a.e41 - b.e41, a.e42 - b.e42, a.e43 - b.e43, a.e44 - b.e44);
        }
        public static Matrix4x4 operator *(Matrix4x4 m, float scalar)
        {
            return new Matrix4x4(
                m.e11 * scalar, m.e12 * scalar, m.e13 * scalar, m.e14 * scalar,
                m.e21 * scalar, m.e22 * scalar, m.e23 * scalar, m.e24 * scalar,
                m.e31 * scalar, m.e32 * scalar, m.e33 * scalar, m.e34 * scalar,
                m.e41 * scalar, m.e42 * scalar, m.e43 * scalar, m.e44 * scalar);
        }
        public static Matrix4x4 operator *(float sclar, Matrix4x4 m)
        {
            return m * sclar;
        }
        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            return new Matrix4x4(
                a.e11 * b.e11 + a.e12 * b.e21 + a.e13 * b.e31 + a.e14 * b.e41,
                a.e11 * b.e12 + a.e12 * b.e22 + a.e13 * b.e32 + a.e14 * b.e42,
                a.e11 * b.e13 + a.e12 * b.e23 + a.e13 * b.e33 + a.e14 * b.e43,
                a.e11 * b.e14 + a.e12 * b.e24 + a.e13 * b.e34 + a.e14 * b.e44,

                a.e21 * b.e11 + a.e22 * b.e21 + a.e23 * b.e31 + a.e24 * b.e41,
                a.e21 * b.e12 + a.e22 * b.e22 + a.e23 * b.e32 + a.e24 * b.e42,
                a.e21 * b.e13 + a.e22 * b.e23 + a.e23 * b.e33 + a.e24 * b.e43,
                a.e21 * b.e14 + a.e22 * b.e24 + a.e23 * b.e34 + a.e24 * b.e44,

                a.e31 * b.e11 + a.e32 * b.e21 + a.e33 * b.e31 + a.e34 * b.e41,
                a.e31 * b.e12 + a.e32 * b.e22 + a.e33 * b.e32 + a.e34 * b.e42,
                a.e31 * b.e13 + a.e32 * b.e23 + a.e33 * b.e33 + a.e34 * b.e43,
                a.e31 * b.e14 + a.e32 * b.e24 + a.e33 * b.e34 + a.e34 * b.e44,

                a.e41 * b.e11 + a.e42 * b.e21 + a.e43 * b.e31 + a.e44 * b.e41,
                a.e41 * b.e12 + a.e42 * b.e22 + a.e43 * b.e32 + a.e44 * b.e42,
                a.e41 * b.e13 + a.e42 * b.e23 + a.e43 * b.e33 + a.e44 * b.e43,
                a.e41 * b.e14 + a.e42 * b.e24 + a.e43 * b.e34 + a.e44 * b.e44);
        }

        public Matrix4x4 Transpose()
        {
            return new Matrix4x4(
                e11, e21, e31, e41,
                e12, e22, e32, e42,
                e13, e23, e33, e43,
                e14, e24, e34, e44);
        }
        public Matrix4x4 Inverse()
        {
            float det = Determinant();
            if (System.MathF.Abs(det) < float.Epsilon)
                throw new InvalidOperationException("This matrix is non-invertible.");

            float invDet = 1.0f / det;

            return new Matrix4x4(
                invDet * (
                    e22 * (e33 * e44 - e34 * e43) -
                    e32 * (e23 * e44 - e24 * e43) +
                    e42 * (e23 * e34 - e24 * e33)),

                invDet * (
                    -(e12 * (e33 * e44 - e34 * e43) -
                    e32 * (e13 * e44 - e14 * e43) +
                    e42 * (e13 * e34 - e14 * e33))),

                invDet * (
                    e12 * (e23 * e44 - e24 * e43) -
                    e22 * (e13 * e44 - e14 * e43) +
                    e42 * (e13 * e24 - e14 * e23)),

                invDet * (
                    -(e12 * (e23 * e34 - e24 * e33) -
                    e22 * (e13 * e34 - e14 * e33) +
                    e32 * (e13 * e24 - e14 * e23))),

                invDet * (
                    -(e21 * (e33 * e44 - e34 * e43) -
                    e31 * (e23 * e44 - e24 * e43) +
                    e41 * (e23 * e34 - e24 * e33))),

                invDet * (
                    e11 * (e33 * e44 - e34 * e43) -
                    e31 * (e13 * e44 - e14 * e43) +
                    e41 * (e13 * e34 - e14 * e33)),

                invDet * (
                    -(e11 * (e23 * e44 - e24 * e43) -
                    e21 * (e13 * e44 - e14 * e43) +
                    e41 * (e13 * e24 - e14 * e23))),

                invDet * (
                    e11 * (e23 * e34 - e24 * e33) -
                    e21 * (e13 * e34 - e14 * e33) +
                    e31 * (e13 * e24 - e14 * e23)),

                invDet * (
                    e21 * (e32 * e44 - e34 * e42) -
                    e31 * (e22 * e44 - e24 * e42) +
                    e41 * (e22 * e34 - e24 * e32)),

                invDet * (
                    -(e11 * (e32 * e44 - e34 * e42) -
                    e31 * (e12 * e44 - e14 * e42) +
                    e41 * (e12 * e34 - e14 * e32))),

                invDet * (
                    e11 * (e22 * e44 - e24 * e42) -
                    e21 * (e12 * e44 - e14 * e42) +
                    e41 * (e12 * e24 - e14 * e22)),

                invDet * (
                    -(e11 * (e22 * e34 - e24 * e32) -
                    e21 * (e12 * e34 - e14 * e32) +
                    e31 * (e12 * e24 - e14 * e22))),

                invDet * (
                    -(e21 * (e32 * e43 - e33 * e42) -
                    e31 * (e22 * e43 - e23 * e42) +
                    e41 * (e22 * e33 - e23 * e32))),

                invDet * (
                    e11 * (e32 * e43 - e33 * e42) -
                    e31 * (e12 * e43 - e13 * e42) +
                    e41 * (e12 * e33 - e13 * e32)),

                invDet * (
                    -(e11 * (e22 * e43 - e23 * e42) -
                    e21 * (e12 * e43 - e13 * e42) +
                    e41 * (e12 * e23 - e13 * e22))),

                invDet * (
                    e11 * (e22 * e33 - e23 * e32) -
                    e21 * (e12 * e33 - e13 * e32) +
                    e31 * (e12 * e23 - e13 * e22)));
        }
        public float Determinant()
        {
            return e11 * (e22 * (e33 * e44 - e34 * e43) - e32 * (e23 * e44 - e24 * e43) + e42 * (e23 * e34 - e24 * e33)) -
                   e12 * (e21 * (e33 * e44 - e34 * e43) - e31 * (e23 * e44 - e24 * e43) + e41 * (e23 * e34 - e24 * e33)) +
                   e13 * (e21 * (e32 * e44 - e34 * e42) - e31 * (e22 * e44 - e24 * e42) + e41 * (e22 * e34 - e24 * e32)) -
                   e14 * (e21 * (e32 * e43 - e33 * e42) - e31 * (e22 * e43 - e23 * e42) + e41 * (e22 * e33 - e23 * e32));
        }
    }
}
