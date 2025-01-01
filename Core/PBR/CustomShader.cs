using NPhotoshop.Core.Image;
using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Core.PBR
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
        public Color[] Run_FragmentShader(Raster[] rasters, Color[] frameBuffer, int width)
        {
            Parallel.For(0, rasters.Length, (idx) =>
            {
                if(rasters[idx].TriangleIndex != -1)
                    frameBuffer[rasters[idx].x + rasters[idx].y * width] = FragmentShader(rasters[idx]);
            });
            return frameBuffer;
        }
        protected abstract Color FragmentShader(Raster raster);
        protected abstract Vector3 VertextShader(Vector3 vertex_position_WorldSpace, Vector3 vertex_normal_WorldSpace, Vector3 objectposition_WorldSpace);
    }
}
