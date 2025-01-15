using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using Athena.Engine.Core;
using Athena.Engine.Core.Rendering;

namespace Athena.MeshLoader
{
    public class FBXLoader
    {
        public static Athena.Engine.Core.MeshRenderer LoadFBX_SeperatedAsRenderer(string filePath)
        {
            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            // FBX 파일을 읽어들임
            Assimp.Scene scene = importer.ImportFile(EngineController.AssetPath + "/" + filePath, Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.GenerateNormals | Assimp.PostProcessSteps.GenerateUVCoords);

            if (scene == null || scene.HasMeshes == false)
            {
                System.Diagnostics.Debug.WriteLine("Warn!");
                return null;
            }

            int vertexCount = 0;
            int triangleCount = 0;
            int meshcount = scene.MeshCount;
            System.Diagnostics.Debug.WriteLine($"Mesh Count : {scene.MeshCount}");

            RenderData[] list = new RenderData[meshcount];
            for (int s = 0; s < meshcount; s++)
            {
                var result = list[s] = new Athena.Engine.Core.Rendering.RenderData();
                var mesh = scene.Meshes[s];
                vertexCount = mesh.VertexCount;
                triangleCount = mesh.FaceCount;

                result.Vertices = new Vertex[vertexCount];
                result.Triangles = new int[triangleCount * 3];

                System.Diagnostics.Debug.WriteLine($"{s} : M{triangleCount} V{vertexCount}");
                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    var vert = mesh.Vertices[i];
                    result.Vertices[i].Position_ObjectSpace = new Vector3(vert.X, vert.Y, vert.Z);
                    //if(mesh.TextureCoordinateChannels.Length > 0)
                    try
                    {
                        result.Vertices[i].UV = new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y);
                    }
                    catch { 
                        result.Vertices[i].UV = new Vector2(0, 0);
                    }
                    result.Vertices[i].Normal_ObjectSpace = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
                }
                for (int i = 0; i < mesh.FaceCount; i++)
                {
                    var face = mesh.Faces[i];
                    result.Triangles[3 * i] = face.Indices[0];
                    result.Triangles[3 * i + 1] = face.Indices[1];
                    result.Triangles[3 * i + 2] = face.Indices[2];
                }
                result.CalculateAABB();
            }
            Athena.Engine.Core.MeshRenderer renderComponent = new Athena.Engine.Core.MeshRenderer();
            renderComponent.RenderDatas.AddRange(list);
            return renderComponent;
        }
    }
}
