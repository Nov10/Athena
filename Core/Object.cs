using Renderer.Core.PBR;
using Renderer.Core.Shader;
using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Renderer
{
    public class Object
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3[] Vertices;
        public Vertex[] Vertices2;
        //public Vector2[] UVs;
        public int[] Triangles;
        public Color[] Colors;

        public CustomShader Shader;
        public FragmentShader FragmentShader;

        public Matrix4x4 CalculateObjectTransformMatrix()
        {
            return TransformMatrixCaculator.CreateTranslationMatrix(Position) * TransformMatrixCaculator.CreateRotationMatrix(Rotation);
        }
    }
}
