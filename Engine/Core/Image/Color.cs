using Athena.Maths;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Engine.Core.Image
{
    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        public static Color black
        {
            get { return new Color(0, 0, 0, 255); }
        }
        public static Color white
        {
            get { return new Color(255, 255, 255, 255); }
        }
        public Vector3 GetAsVector3()
        {
            return new Vector3(R, G, B);
        }
        public override string ToString()
        {
            return $"({R}, {G}, {B}, {A})";
        }
        public static Color Lerp(Color a, Color b, float t)
        {
            return (1 - t) * a + t * b;
        }
        public static Color operator +(Color left, Color right)
        {
            return new Color(
                (byte)(left.R + right.R),
                (byte)(left.G + right.G),
                (byte)(left.B + right.B),
                (byte)(left.A + right.A));
        }
        public static Color operator *(Color left, float right)
        {
            return new Color(
                (byte)(left.R * right),
                (byte)(left.G * right),
                (byte)(left.B * right),
                (byte)(left.A * right));
        }
        public static Color operator *(float left, Color right)
        {
            return right * left;
        }
        public static bool operator ==(Color left, Color right)
        {
            if (left.A != right.A) return false;
            if (left.B != right.B) return false;
            if (left.R != right.R) return false;
            if (left.G != right.G) return false;
            return true;
        }
        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
