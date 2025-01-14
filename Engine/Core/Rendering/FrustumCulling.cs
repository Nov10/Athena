using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using Microsoft.UI.Xaml.Media;

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
    /// <summary>
    /// 프러스텀 컬링을 위한 유틸리티
    /// </summary>
    public static class FrustumCulling
    {
        /// <summary>
        /// 컬링 여부를 결정합니다.
        /// </summary>
        /// <param name="aabb"></param>
        /// <param name="cameraPosition"></param>
        /// <param name="objectPosition"></param>
        /// <param name="cameraTransform"></param>
        /// <param name="objectInvTransform"></param>
        /// <returns></returns>
        public static bool Culling(AABB aabb, Vector3 cameraPosition, Vector3 objectPosition, Matrix4x4 cameraTransform, Matrix4x4 objectInvTransform)
        {
            //아래 두 함수는 해도 안 해도 컬링 여부에는 결과를 미치지 않는다.
            //그러나, 미리 여기서 간단한 것들을 계산하고 넘어가면, 뒤쪽의 무거운 연산을 줄일 수 있다.
            if (Culling_IsObjectPositionViewed(objectPosition, cameraTransform))
                return true;
            if (Culling_IsCameraInsedAABB(cameraPosition, aabb, objectInvTransform))
                return true;

            //AABB의 코너 중 단 하나라고 절두체에 있으면, 렌더링
            var corners = aabb.GetCorners();
            Vector4[] transformedCorners = new Vector4[8];
            for (int i = 0; i < 8; i++)
            {
                var corner = transformedCorners[i] = TransformMatrixCaculator.TransformH(corners[i], cameraTransform);
                float w = corner.w;
                //하나라도 프러스텀 안에 있다면 컬링되지 않음
                if (-w <= corner.x && corner.x <= w
                 && -w <= corner.y && corner.y <= w
                 && -w <= corner.z && corner.z <= w)
                {
                    return true;
                }
            }

            //정확하게 컬링
            if(Culling_6Faced(aabb, transformedCorners))
                return true;
            return false;
        }
        /// <summary>
        /// 절두체의 6면에 대하여 컬링을 계산합니다.
        /// </summary>
        /* ClipSpace에서도 절두체에 대한 컬링을 계산할 수 있다.
         * 
         * 방식은 WorldSpace에서 수행하는 것과 똑같다.
         * 기준을 AABB의 코너로 잡고 컬링한다.
         * 
         * AABB의 각 코너에 대해, LocalSpace -> ClipSpace로의 변환을 적용한다.
         * 그리고 변환된 코너와 가장 가까운 절두체 평면을 찾는다.
         * 이 과정은 변환된 코너의 x, y, z의 절댓값 중 가장 작은 원소가 무엇인지에 따라 계산할 수 있다. 만일 abs(x)가 가장 작다면, 변환된 코너는 (1, 0, 0, 1)이나 (-1, 0, 0, 1) 둘 중 하나에 가장 가깝게 위치한다.
         * 그리고 ClipSpace -> NDC로의 변환을 적용하여 (x / w), 그 부호에 따라 어떤 평면과 가장 가까운지 계산한다.
         * 
         * 변환된 코너와 그 코너와 가장 가까운 평면에 대해, 코너가 평면 안에 있는지 확인한다. 만일 안에 있다면 렌더링해야 하므로, true 반환
         * 만약 바깥에 있다면, 코터의 반대 코너에 대해 같은 연산을 수행한다. 이는 Intersect인 경우를 판별하기 위함이다. 단, 평면은 업데이트 하면 안된다. 반대 코너와 가장 가까운 평면와 계산한다고 해서, 원래 코너와 함께 Intersect를 판별할 수 없다.
         * 만일 반대 코너가 안쪽에 있다면, 이는 Intersect, 즉 AABB가 프러스텀에 걸쳐 있는 상황이다. 따라서 이 경우도 렌더링해야 하므로, true 반환.
         * 
         * 그 외의 경우는 false를 반환하여 렌더링하지 않아야 한다.
         * 
         * WorldSpace에서 ClipSpace로 넘어오는 순간 절두체를 구성하는 평면의 법선이 반드시 정해지므로, WorldSpace에서 절두체 평면의 법선을 계산하던 것을 하지 않아도 된다.
         */
        static bool Culling_6Faced(AABB aabb, Vector4[] transformedCorners)
        {
            /// AABB의 꼭짓점 중 하나를 가져옵니다.
            /// 매개변수의 각 값이 0이면 그 원소에 대해 min을, 1이면 max에 해당하는 꼭짓점의 인덱스를 반환합니다.
            int GetAABBCorner(int n_x, int n_y, int n_z)
            {
                //0 : min
                //1 : max
                switch((n_x, n_y, n_z))
                {
                    case (0, 0, 0):
                        return 0;
                    case (0, 0, 1):
                        return 1;
                    case (0, 1, 0):
                        return 2;
                    case (0, 1, 1):
                        return 3;

                    case (1, 0, 0):
                        return 4;
                    case (1, 0, 1):
                        return 5;
                    case (1, 1, 0):
                        return 6;
                    case (1, 1, 1):
                        return 7;
                }
                return 0;
            }
            /// v의 x, y, z의 절댓값 중 가장 작은 값을 찾습니다.
            /// x : 0, y : 1, z : 2
            int GetMinElementIndex(Vector4 v)
            {   
                float absX = System.MathF.Abs(v.x);
                float absY = System.MathF.Abs(v.y);
                float absZ = System.MathF.Abs(v.z);

                float minValue = absX;
                int minIndex = 0;

                if (absY < minValue)
                {
                    minValue = absY;
                    minIndex = 1;
                }

                if (absZ < minValue)
                {
                    minValue = absZ;
                    minIndex = 2;
                }

                return minIndex;
            }

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        var p = transformedCorners[GetAABBCorner(x, y, z)];
                        int plane = 0;
                        switch(GetMinElementIndex(p))
                        {
                            //원래 ClipSpace -> NDC는 w로 나누어야 하지만,
                            //부호가 중요하므로 곱하여도 무방합니다.
                            case 0:
                                if (p.x * p.w > 0)
                                    plane = 0;
                                else
                                    plane = 1;
                                break;
                            case 1:
                                if (p.y * p.w > 0)
                                    plane = 2;
                                else
                                    plane = 3;
                                break;
                            case 2:
                                if (p.z * p.w > 0)
                                    plane = 4;
                                else
                                    plane = 5;
                                break;
                        }

                        if (IsInsidePlane(p, GetClipPlane(plane)) == true)
                            return true;

                        int px = x == 0 ? 1 : 0;
                        int py = y == 0 ? 1 : 0;
                        int pz = z == 0 ? 1 : 0;
                        p = transformedCorners[GetAABBCorner(px, py, pz)];

                        if (IsInsidePlane(p, GetClipPlane(plane)) == false)
                            return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 카메라가 AABB의 내부에 있는지 계산하여 컬링 여부를 결정합니다.
        /// </summary>
        static bool Culling_IsCameraInsedAABB(Vector3 cameraPosition, AABB aabb, Matrix4x4 objectInvTransform)
        {
            Vector4 p = TransformMatrixCaculator.TransformH(cameraPosition, objectInvTransform);
            float w = p.w;
            if(    w * aabb.Min.x <= p.x && p.x <= w * aabb.Max.x
                && w * aabb.Min.y <= p.y && p.y <= w * aabb.Max.y
                && w * aabb.Min.z <= p.x && p.z <= w * aabb.Max.z)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 물체의 중심점이 보이는지 계산하여 컬링 여부를 결정합니다.
        /// </summary>
        static bool Culling_IsObjectPositionViewed(Vector3 objectPosition, Matrix4x4 cameraTransform)
        {
            Vector4 p = TransformMatrixCaculator.TransformH(objectPosition, cameraTransform);
            float w = p.w;
            if (-w <= p.x && p.x <= w
             && -w <= p.y && p.y <= w
             && -w <= p.z && p.z <= w)
            {
                return true;
            }
            return false;
        }
        static Vector4 GetClipPlane(int index)
        {
            return index switch
            {
                0 => new Vector4(1, 0, 0, 1),
                1 => new Vector4(-1, 0, 0, 1),
                2 => new Vector4(0, 1, 0, 1),
                3 => new Vector4(0, -1, 0, 1),
                4 => new Vector4(0, 0, 1, 1),
                5 => new Vector4(0, 0, -1, 1),
                _ => throw new ArgumentException("Invalid clip plane index")
            };
        }
        static bool IsInsidePlane(Vector4 point, Vector4 plane)
        {
            return point.x * plane.x + point.y * plane.y + point.z * plane.z + point.w * plane.w >= 0;
        }
    }
}

