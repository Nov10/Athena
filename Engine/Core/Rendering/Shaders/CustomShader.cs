using Athena.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Engine.Core.Image;

namespace Athena.Engine.Core.Rendering.Shaders
{
    public abstract class CustomShader
    {
        public Vertex[] Run_VertexShader(Vertex[] vertices, Vector3 objectposition)
        {
            Parallel.For(0, vertices.Length, (idx) =>
            {
                vertices[idx].Position_WorldSpace = VertextShader(vertices[idx].Position_WorldSpace, vertices[idx].Normal_WorldSpace, objectposition);
            });
            return vertices;
        }

        public Color[] Run_FragmentShader(Raster[] rasters, Color[] frameBuffer, Vector3 lightDir, int width)
        {
            Parallel.For(0, rasters.Length, (idx) =>
            {
                if (rasters[idx].TriangleIndex != -1)
                    frameBuffer[rasters[idx].x + rasters[idx].y * width] = FragmentShader(rasters[idx], lightDir);
            });
            return frameBuffer;
        }
        public abstract Color FragmentShader(Raster raster, Vector3 light);
        public abstract Vector3 VertextShader(Vector3 vertex_position_WorldSpace, Vector3 vertex_normal_WorldSpace, Vector3 objectposition_WorldSpace);
    }
}
