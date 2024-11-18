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
                    frameBuffer[rasters[idx].x + rasters[idx].y * width] = FragmentShader(rasters[idx].x, rasters[idx].y, rasters[idx].Position_ScreenVolumeSpace, rasters[idx].Normal_WorldSpace, rasters[idx].Lambda, rasters[idx].UV);
            });
            return frameBuffer;
        }
        protected abstract Color FragmentShader(int screen_x, int screen_y, Vector3 position_ScreenSpace, Vector3 normal, Vector3 lambda, Vector2 uv);
        protected abstract Vector3 VertextShader(Vector3 vertex_position_WorldSpace, Vector3 vertex_normal_WorldSpace, Vector3 objectposition_WorldSpace);
    }
}
