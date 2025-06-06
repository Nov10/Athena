﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Maths
{
    public class XMath
    {
        public const float Deg2Rad = 0.01745329251994329576923690768488612713442871888541725456097191440171009114f;
        public const float Deg2Rad_Half = 0.00872664625997164788461845384244306356721435944270862728048595720085504557f;
        public const float Rad2Deg = 57.2957795130823208767981548141051703324054724665643215491602438612028471483f;
        public const float Rad2Deg_Half = 28.6478897565411604383990774070525851662027362332821607745801219306014235741f;

        public static float InverseLerp(float a, float b, float value)
        {
            return (value - a) / (b - a);
        }

        public static float Lerp(float a, float b, float value)
        {
            return (1 - value) * a + value * b;
        }

        public static float Clamp(float value, float min, float max)
        {
            return MathF.Max(min, MathF.Min(value, max));
        }

        public static int RoundToInt(float value)
        {
            return (int)MathF.Round(value);
        }
        public static int FloorToInt(float value)
        {
            return (int)MathF.Floor(value);
        }
        public static int CeilToInt(float value)
        {
            return (int)MathF.Ceiling(value);
        }
    }
}
