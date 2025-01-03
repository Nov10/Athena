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
    public class RenderData
    {
        //public Vector3[] Vertices;
        public Vertex[] Vertices2;
        //public Vector2[] UVs;
        public int[] Triangles;
        public Color[] Colors;

        public CustomShader Shader;
        public FragmentShader FragmentShader;


        public RenderData(RenderData data)
        {
            //this.Vertices = (Vector3[]) data.Vertices.Clone();
            this.Vertices2 = (Vertex[])data.Vertices2.Clone();
            this.Triangles = (int[])data.Triangles.Clone();
            this.Colors = data.Colors;
            this.Shader = data.Shader;
            this.FragmentShader = data.FragmentShader;
        }
        public RenderData()
        {

        }

    }
}
