using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;

namespace Athena.Engine.Core.Rendering
{        
    /// <summary>
    /// 축 정렬 바운딩 박스(AABB)
    /// </summary>
    public struct AABB
    {
        public Vector3 Min;
        public Vector3 Max;

        public AABB(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }
        public AABB(Vertex[] vertices)
        {
            if (vertices == null || vertices.Length == 0)
            {
                Max = Min = Vector3.zero;
            }
            else
            {
                Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                foreach (var v in vertices)
                {
                    Vector3 p = v.Position_ObjectSpace;
                    if (p.x < min.x) min.x = p.x;
                    if (p.y < min.y) min.y = p.y;
                    if (p.z < min.z) min.z = p.z;

                    if (p.x > max.x) max.x = p.x;
                    if (p.y > max.y) max.y = p.y;
                    if (p.z > max.z) max.z = p.z;
                }

                Max = max;
                Min = min;
            }
        }

        public Vector3[] GetCorners()
        {
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(Min.x, Min.y, Min.z);
            corners[1] = new Vector3(Min.x, Min.y, Max.z);
            corners[2] = new Vector3(Min.x, Max.y, Min.z);
            corners[3] = new Vector3(Min.x, Max.y, Max.z);
            corners[4] = new Vector3(Max.x, Min.y, Min.z);
            corners[5] = new Vector3(Max.x, Min.y, Max.z);
            corners[6] = new Vector3(Max.x, Max.y, Min.z);
            corners[7] = new Vector3(Max.x, Max.y, Max.z);
            return corners;
        }
    }
    /// <summary>
    /// 프러스텀 컬링을 위한 유틸리티
    /// </summary>
    public static class FrustumCulling
    {
        public static bool Culling(AABB aabb, Matrix4x4 transform)
        {
            var corners = aabb.GetCorners();
            foreach (var corner in corners)
            {
                var clipPoint = TransformMatrixCaculator.TransformH(corner, transform);
                float w = clipPoint.w;
                //하나라도 프러스텀 안에 있다면 컬링되지 않음
                if (-w <= clipPoint.x && clipPoint.x <= w
                 && -w <= clipPoint.y && clipPoint.y <= w
                 && -w <= clipPoint.z && clipPoint.z <= w)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

