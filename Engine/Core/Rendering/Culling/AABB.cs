using Athena.Maths;

namespace Athena.Engine.Core.Rendering
{
    /// <summary>
    /// Axis Aligned Bounding Box (AABB, 축 정렬 바운딩 박스)
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

        /// <summary>
        /// AABB의 8개 꼭짓점을 가져옵니다. 인덱스는 다음과 같습니다.
        ///<para>0 : 왼쪽 아래 앞</para>
        ///<para>1 : 왼쪽 아래 뒤</para>
        ///<para>2 : 왼쪽 위 앞</para>
        ///<para>3 : 왼쪽 위 뒤</para>
        ///<para>4 : 오른쪽 아래 앞</para>
        ///<para>5 : 오른쪽 아래 뒤</para>
        ///<para>6 : 오른쪽 위 앞</para>
        ///<para>7 : 오른쪽 위 뒤</para>
        /// </summary>
        /// <returns></returns>
        public readonly Vector3[] GetCorners()
        {
            //왼쪽 아래 앞
            //왼쪽 아래 뒤
            //왼쪽 위 앞
            //왼쪽 위 뒤
            //오른쪽 아래 앞
            //오른쪽 아래 뒤
            //오른쪽 위 앞
            //오른쪽 위 뒤
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
}
