using NPhotoshop.Core.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renderer.Maths;

namespace Renderer.Core.Shader
{
    public struct FragmentShader
    {
        public Color Calculate(int x, int y, float z, float lambda1, float lambda2, float lambda3, Vertex p1, Vertex p2, Vertex p3)
        {
            Vector3 normalInterpolated = lambda1 * p1.Normal_WorldSpace + lambda2 * p2.Normal_WorldSpace + lambda3 * p3.Normal_WorldSpace;
            float brightness = Vector3.Dot(normalInterpolated.normalized, -(new Vector3(-1f, -1, 0)).normalized);
            brightness = 0.5f * brightness + 0.5f;
            byte intensity = (byte)(brightness * 255);

            var color = new NPhotoshop.Core.Image.Color(intensity, intensity, intensity, 255);

            return color;
        }
    }
}
