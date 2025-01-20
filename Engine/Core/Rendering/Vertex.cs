using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;

namespace Athena.Engine.Core.Rendering
{
    public struct Vertex
    {
        public Vector3 Position_ObjectSpace;
        public Vector3 Position_WorldSpace;
        public Vector3 Position_ScreenVolumeSpace;
        public Vector3 Normal_ObjectSpace;
        public Vector3 Normal_WorldSpace;
        public Vector3 LightViewPosition;
        public Vector2 UV;

        public Vector4 ClipPoint;

        public Vector3 Tangent;
        public Vector3 Bitangent;
    }
}
