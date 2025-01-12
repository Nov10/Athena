using Athena.Maths;
using Athena.Engine.Core.Rendering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Engine.Core.Rendering
{
    public class RenderData
    {
        public Vertex[] Vertices;
        public int[] Triangles;
        public Color[] Colors;

        public CustomShader Shader;

        public AABB ThisAABB;

        public void CalculateNormals()
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal_ObjectSpace = new Vector3();
            }
            for (int i = 0; i < Triangles.Length / 3; i++)
            {
                var p1 = Vertices[Triangles[3 * i]];
                var p2 = Vertices[Triangles[3 * i + 1]];
                var p3 = Vertices[Triangles[3 * i + 2]];

                Vector3 normal = Vector3.Cross(p2.Position_ObjectSpace - p1.Position_ObjectSpace, p3.Position_ObjectSpace - p1.Position_ObjectSpace);
                p1.Normal_ObjectSpace += normal;
                p2.Normal_ObjectSpace += normal;
                p3.Normal_ObjectSpace += normal;

                Vertices[Triangles[3 * i]] = p1;
                Vertices[Triangles[3 * i + 1]] = p2;
                Vertices[Triangles[3 * i + 2]] = p3;
            }
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal_ObjectSpace = Vertices[i].Normal_ObjectSpace.normalized;
            }
        }

        public void CalculateAABB()
        {
            ThisAABB = new AABB(Vertices);
        }

        public RenderData(RenderData data)
        {
            //this.Vertices = (Vector3[]) data.Vertices.Clone();
            Vertices = (Vertex[])data.Vertices.Clone();
            Triangles = (int[])data.Triangles.Clone();
            Colors = data.Colors;
            Shader = data.Shader;
            ThisAABB = data.ThisAABB;
        }
        public RenderData()
        {

        }

    }
}
