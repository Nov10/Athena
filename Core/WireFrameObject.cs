using Renderer.Maths;
using Renderer.Renderer.PBR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Renderer
{
    public class WireFrameObject : PBRObject
    {

    //    public List<Vector3> cubeVertices = new List<Vector3>
    //        {
    //            new Vector3(2.0f, 2.0f, 2.0f),
    //            new Vector3(2.0f, 2.0f, -2.0f),
    //            new Vector3(2.0f, -2.0f, -2.0f),
    //            new Vector3(2.0f, -2.0f, 2.0f),
    //            new Vector3(-2.0f, 2.0f, 2.0f),
    //            new Vector3(-2.0f, 2.0f, -2.0f),
    //            new Vector3(-2.0f, -2.0f, -2.0f),
    //            new Vector3(-2.0f, -2.0f, 2.0f),
    //        };

    //    public List<Tuple<int, int>> cubeEdges = new List<Tuple<int, int>>
    //        {
    //            Tuple.Create(0, 1), Tuple.Create(1, 2), Tuple.Create(2, 3), Tuple.Create(3, 0),
    //            Tuple.Create(4, 5), Tuple.Create(5, 6), Tuple.Create(6, 7), Tuple.Create(7, 4),
    //            Tuple.Create(0, 4), Tuple.Create(1, 5), Tuple.Create(2, 6), Tuple.Create(3, 7)
    //        };
    //    public int[] Triangles = new int[] {
    //// 앞면
    //0, 3, 2,
    //0, 2, 1, 
    
    //// 뒷면
    //4, 5, 6,
    //4, 6, 7, 
    
    //// 윗면
    //0, 1, 5,
    //0, 5, 4, 
    
    //// 아랫면
    //2, 3, 7,
    //2, 7, 6, 
    
    //// 왼쪽면
    //4, 7, 3,
    //4, 3, 0, 
    
    //// 오른쪽면
    //1, 2, 6,
    //1, 6, 5
    //};
    }
}
