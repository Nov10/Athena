using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;

namespace Athena.Engine.Core.Rendering
{
    public struct Raster
    {
        public int x;
        public int y;
        public int TriangleIndex;
        public Vector3 Normal_WorldSpace;
        public Vector2 UV;
        public Vector3 Tangent;
        public Vector3 BitTangent;

        public Raster(int x, int y, int triangleIndex, Vector3 normal_WorldSpace, Vector2 uv)
        {
            this.x = x;
            this.y = y;
            TriangleIndex = triangleIndex;
            Normal_WorldSpace = normal_WorldSpace;
            UV = uv;
        }
    }
}
