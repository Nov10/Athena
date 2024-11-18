using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPhotoshop.Core.Image
{
    public struct Color
    {
        public override string ToString()
        {
            return $"({R}, {G}, {B}, {A})";
        }

        public static bool operator ==(Color left, Color right)
        {
            if(left.A != right.A) return false;
            if(left.B != right.B) return false;
            if(left.R != right.R) return false;
            if(left.G != right.G) return false;
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
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }
    }
}
