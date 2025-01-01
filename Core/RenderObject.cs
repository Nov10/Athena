using Renderer.Core.PBR;
using Renderer.Core.Shader;
using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Renderer
{
    //public class MultipleMeshObject : RenderObject
    //{
    //    public int ObjectCount
    //    {
    //        get { return Objects.Length; }
    //    }
    //    //public Matrix4x4 CalculateObjectTransformMatrix()
    //    //{
    //    //    return TransformMatrixCaculator.CreateTranslationMatrix(Position) * TransformMatrixCaculator.CreateRotationMatrix(Rotation);
    //    //}

    //    //public Vector3 Position;
    //    //public Vector3 Rotation;

    //    public RenderObject Get(int idx)
    //    {
    //        return Objects[idx];
    //    }

    //    public RenderObject[] Objects;
    //    public MultipleMeshObject()
    //    {
    //        Objects = new RenderObject[0];
    //    }

    //    public MultipleMeshObject(RenderObject[] objects)
    //    {
    //        Objects = objects;
    //    }
    //}
    public class RenderObject
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
            return TransformMatrixCaculator.CreateTranslationMatrix(Position) * TransformMatrixCaculator.CreateRotationMatrix(Rotation).Transpose();
        }

    }
}
