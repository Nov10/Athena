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

        public void CalculateNormals()
        {
            for(int i = 0; i < Vertices2.Length; i++)
            {
                Vertices2[i].Normal_ObjectSpace = new Vector3();
            }
            for(int i = 0; i < Triangles.Length/3; i++)
            {
                var p1 = Vertices2[Triangles[3 * i]];
                var p2 = Vertices2[Triangles[3 * i+1]];
                var p3 = Vertices2[Triangles[3 * i+2]];

                Vector3 normal = Vector3.Cross(p2.Position_ObjectSpace - p1.Position_ObjectSpace, p3.Position_ObjectSpace - p1.Position_ObjectSpace);
                p1.Normal_ObjectSpace += normal;
                p2.Normal_ObjectSpace += normal;
                p3.Normal_ObjectSpace += normal;

                Vertices2[Triangles[3 * i]] = p1;
                Vertices2[Triangles[3 * i + 1]] = p2;
                Vertices2[Triangles[3 * i + 2]]  = p3;
            }
            for (int i = 0; i < Vertices2.Length; i++)
            {
                Vertices2[i].Normal_ObjectSpace = Vertices2[i].Normal_ObjectSpace.normalized;
            }
        }


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
