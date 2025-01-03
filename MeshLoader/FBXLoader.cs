using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renderer.Maths;
using Renderer.Renderer;
using Renderer.Renderer.PBR;
using Renderer.Core;

namespace Renderer.MeshLoader
{
    public class FBXLoader
    {
        public static Core.Renderer LoadFBX_SeperatedAsRenderer(string filePath)
        {
            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            // FBX 파일을 읽어들임
            Assimp.Scene scene = importer.ImportFile(filePath, Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.GenerateNormals | Assimp.PostProcessSteps.GenerateUVCoords);

            if (scene == null || scene.HasMeshes == false)
            {
                System.Diagnostics.Debug.WriteLine("Warn!");
                return null;
            }

            int vertexCount = 0;
            int triangleCount = 0;
            int meshcount = scene.MeshCount;
            System.Diagnostics.Debug.WriteLine($"Mesh Count : {scene.MeshCount}");

            Renderer.RenderData[] list = new Renderer.RenderData[meshcount];
            for (int s = 0; s < meshcount; s++)
            {
                var result = list[s] = new Renderer.RenderData();
                var mesh = scene.Meshes[s];
                vertexCount = mesh.VertexCount;
                triangleCount = mesh.FaceCount;

                result.Vertices2 = new Vertex[vertexCount];
                //result.Vertices = new Vector3[vertexCount];
                result.Triangles = new int[triangleCount * 3];
                result.Colors = new System.Drawing.Color[triangleCount];

                System.Diagnostics.Debug.WriteLine($"{s} : M{triangleCount} V{vertexCount}");
                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    var vert = mesh.Vertices[i];
                    //result.Vertices[counter_vert] = new Vector3(vert.X, vert.Y, vert.Z);
                    result.Vertices2[i].Position_ObjectSpace = new Vector3(vert.X, vert.Y, vert.Z);
                    //if(mesh.TextureCoordinateChannels.Length > 0)
                    try
                    {
                        result.Vertices2[i].UV = new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y);
                    }
                    catch { 
                        result.Vertices2[i].UV = new Vector2(0, 0);
                    }
                    result.Vertices2[i].Normal_ObjectSpace = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
                }
                for (int i = 0; i < mesh.FaceCount; i++)
                {
                    var face = mesh.Faces[i];
                    result.Triangles[3 * i] = face.Indices[0];
                    result.Triangles[3 * i + 1] = face.Indices[1];
                    result.Triangles[3 * i + 2] = face.Indices[2];
                }
                //list[s].Position = new Vector3();
                //list[s].Rotation = new Vector3();
            }
            Core.Renderer renderComponent = new Core.Renderer();
            renderComponent.RenderDatas.AddRange(list);
            return renderComponent;
            //return new MultipleMeshObject(list);
        }
        //public static Renderer.MultipleMeshObject LoadFBX_Seperated(string filePath)
        //{
        //    Assimp.AssimpContext importer = new Assimp.AssimpContext();
        //    // FBX 파일을 읽어들임
        //    Assimp.Scene scene = importer.ImportFile(filePath, Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.GenerateNormals | Assimp.PostProcessSteps.GenerateUVCoords);

        //    if (scene == null || scene.HasMeshes == false)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Warn!");
        //        return null;
        //    }

        //    int vertexCount = 0;
        //    int triangleCount = 0;
        //    int meshcount = scene.MeshCount;
        //    System.Diagnostics.Debug.WriteLine($"Mesh Count : {scene.MeshCount}");

        //    Renderer.RenderObject[] list = new Renderer.RenderObject[meshcount];
        //    for(int s = 0; s < meshcount; s++)
        //    {
        //        var result = list[s] = new Renderer.RenderObject();
        //        var mesh = scene.Meshes[s];
        //        vertexCount = mesh.VertexCount;
        //        triangleCount = mesh.FaceCount;

        //        result.Vertices2 = new Vertex[vertexCount];
        //        result.Vertices = new Vector3[vertexCount];
        //        result.Triangles = new int[triangleCount * 3];
        //        result.Colors = new System.Drawing.Color[triangleCount];

        //        System.Diagnostics.Debug.WriteLine($"{s} : M{triangleCount} V{vertexCount}");
        //        for (int i = 0; i < mesh.VertexCount; i++)
        //        {
        //            var vert = mesh.Vertices[i];
        //            //result.Vertices[counter_vert] = new Vector3(vert.X, vert.Y, vert.Z);
        //            result.Vertices2[i].Position_ObjectSpace = new Vector3(vert.X, vert.Y, vert.Z);
        //            result.Vertices2[i].UV = new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y);
        //            result.Vertices2[i].Normal_ObjectSpace = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
        //        }
        //        for (int i = 0; i < mesh.FaceCount; i++)
        //        {
        //            var face = mesh.Faces[i];
        //            result.Triangles[3 * i] = face.Indices[0] ;
        //            result.Triangles[3 * i + 1] = face.Indices[1] ;
        //            result.Triangles[3 * i + 2] = face.Indices[2] ;
        //        }
        //        list[s].Position = new Vector3();
        //        list[s].Rotation = new Vector3();
        //    }

        //    return new MultipleMeshObject(list);
        //}
        public static WireFrameObject LoadFBX_WireFrameObject(string filePath)
        {
            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            // FBX 파일을 읽어들임
            Assimp.Scene scene = importer.ImportFile(filePath, Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.GenerateNormals | Assimp.PostProcessSteps.GenerateUVCoords);

            if (scene == null || scene.HasMeshes == false)
            {
                System.Diagnostics.Debug.WriteLine("Warn!");
                return null;
            }
            WireFrameObject result = new Renderer.WireFrameObject();

            int vertexCount = 0;
            int triangleCount = 0;
            int meshcount = scene.MeshCount;
            System.Diagnostics.Debug.WriteLine($"Mesh Count : {scene.MeshCount}");
            for (int i = 0; i < meshcount; i++)
            {
                vertexCount += scene.Meshes[i].VertexCount;
                triangleCount += scene.Meshes[i].FaceCount;
            }
            result.Vertices2 = new Vertex[vertexCount];
            //result.Vertices = new Vector3[vertexCount];
            result.Triangles = new int[triangleCount * 3];
            result.Colors = new System.Drawing.Color[triangleCount];
            int counter_vert = 0;
            int counter_face = 0;
            int c = 0;

            for (int m = 0; m < meshcount; m++)
            {
                var mesh = scene.Meshes[m];
                System.Diagnostics.Debug.WriteLine($"UV Channel Count: {mesh.TextureCoordinateChannelCount}");

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    var vert = mesh.Vertices[i];
                    //result.Vertices[counter_vert] = new Vector3(vert.X, vert.Y, vert.Z);
                    result.Vertices2[counter_vert].Position_ObjectSpace = new Vector3(vert.X, vert.Y, vert.Z);
                    result.Vertices2[counter_vert].UV = new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y);
                    //System.Diagnostics.Debug.WriteLine(result.Vertices2[counter_vert].UV);
                    //Vector3 n = new Vector3();
                    //Vector3D n2 = new Vector3D(0,0,0);
                    //for (int f = 0; f < mesh.FaceCount; f++)
                    //{
                    //    var face = mesh.Faces[f];
                    //    if(face.Indices[0] == i || face.Indices[1] == i || face.Indices[2] == i)
                    //    {
                    //        var p1 = mesh.Vertices[face.Indices[0]];
                    //        var p2 = mesh.Vertices[face.Indices[1]];
                    //        var p3 = mesh.Vertices[face.Indices[2]];
                    //        var e1 = p2 - p1;
                    //        var e2 = p3 - p1;
                    //        var normal = Assimp.Vector3D.Cross(e1, e2);
                    //        normal.Normalize();
                    //        n2 += normal;
                    //    }
                    //}
                    //n2.Normalize();
                    //result.Vertices2[counter_vert].Normal_ObjectSpace = new Vector3(n2.X, n2.Y, n2.Z);
                    result.Vertices2[counter_vert].Normal_ObjectSpace = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
                    counter_vert++;
                }
                for (int i = 0; i < mesh.FaceCount; i++)
                {
                    var face = mesh.Faces[i];
                    result.Triangles[3 * counter_face] = face.Indices[0] + c;
                    result.Triangles[3 * counter_face + 1] = face.Indices[1] + c;
                    result.Triangles[3 * counter_face + 2] = face.Indices[2] + c;
                    result.Colors[counter_face] = System.Drawing.Color.FromArgb(255, 80, 80, 80);
                    counter_face++;
                }
                c = counter_vert;
            }
            //result.Position = new Vector3();
            //result.Rotation = new Vector3();
            return result;
        }
    }
}
