using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Maths
{
    public struct Quaternion
    {
        public float x; public float y; public float z; public float w = 1;
        public override string ToString()
        {
            return $"{x} {y} {z} {w}";
        }
        public Quaternion()
        {
            w = 1;
            x = y = z = 0;
        }
        // 벡터를 쿼터니언으로 변환하는 함수
        public static Quaternion FromVectorToQuaternion(Vector3 fromVector, Vector3 toVector)
        {
            Vector3 from = fromVector.normalized;
            Vector3 to = toVector.normalized;

            // 벡터 간의 점곱 계산
            float dot = Vector3.Dot(from, to);
            Vector3 cross = Vector3.Cross(from, to);

            // 두 벡터가 동일한 방향이면 단위 쿼터니언 반환
            if (dot > 0.9999f) return new Quaternion(1, 0, 0, 0);

            // 두 벡터가 정반대 방향일 경우, 교차곱을 사용해 회전축 설정
            if (dot < -0.9999f)
            {
                Vector3 orthoAxis = new Vector3(1, 0, 0);
                if (System.Math.Abs(from.x) > System.Math.Abs(from.z))
                {
                    orthoAxis = new Vector3(0, 0, 1);
                }
                return CreateRotationQuaternion(orthoAxis, 180);
            }

            // 두 벡터 사이의 회전각을 사용한 쿼터니언 계산
            float s = (float)System.Math.Sqrt((1 + dot) * 2);
            float invS = 1 / s;
            return new Quaternion(s * 0.5f, cross.x * invS, cross.y * invS, cross.z * invS);
        }
        public Quaternion(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        // 쿼터니언 곱셈
        public static Quaternion operator *(Quaternion q1, Quaternion q2)
        {
            float nw = q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z;
            float nx = q1.w * q2.x + q1.x * q2.w + q1.y * q2.z - q1.z * q2.y;
            float ny = q1.w * q2.y - q1.x * q2.z + q1.y * q2.w + q1.z * q2.x;
            float nz = q1.w * q2.z + q1.x * q2.y - q1.y * q2.x + q1.z * q2.w;
            return new Quaternion(nw, nx, ny, nz);
        }

        // 쿼터니언의 켤레
        public Quaternion Conjugate()
        {
            return new Quaternion(w, -x, -y, -z);
        }

        // 벡터 회전
        public Vector3 RotateVector(Vector3 v)
        {
            Quaternion qVec = new Quaternion(0, v.x, v.y, v.z);
            Quaternion qConj = this.Conjugate();
            Quaternion qRes = this * qVec * qConj;
            return new Vector3( qRes.x, qRes.y, qRes.z );
        }

        // 주어진 축과 각도에 따른 회전 쿼터니언 생성
        public static Quaternion CreateRotationQuaternion(Vector3 axis, float angleDeg)
        {
            axis = axis.normalized;
            float angleRad = (float)(System.Math.PI * angleDeg / 180.0);
            float halfAngle = angleRad / 2;
            float sinHalfAngle = (float)System.Math.Sin(halfAngle);
            float cosHalfAngle = (float)System.Math.Cos(halfAngle);

            return new Quaternion(cosHalfAngle, axis.x * sinHalfAngle, axis.y * sinHalfAngle, axis.z * sinHalfAngle);
        }
        public Quaternion Normalize()
        {
            float magnitude = (float)Math.Sqrt(w * w + x * x + y * y + z * z);
            if (magnitude > 0.0001f)
            {
                w /= magnitude;
                x /= magnitude;
                y /= magnitude;
                z /= magnitude;
            }
            return this;
        }
        public static Quaternion Lerp(Quaternion q1, Quaternion q2, float t)
        {
            if (t < 0 || t > 1)
                throw new ArgumentOutOfRangeException(nameof(t), "t must be in the range [0, 1].");

            // Linear interpolation
            Quaternion result = new Quaternion(
                (1 - t) * q1.w + t * q2.w,
                (1 - t) * q1.x + t * q2.x,
                (1 - t) * q1.y + t * q2.y,
                (1 - t) * q1.z + t * q2.z
            );

            // Normalize the result to ensure it remains a valid quaternion
            result.Normalize();
            return result;
        }
        public static Quaternion Slerp(Quaternion q1, Quaternion q2, float t)
        {
            if (t < 0 || t > 1)
                throw new ArgumentOutOfRangeException(nameof(t), "t must be in the range [0, 1].");

            // Normalize the quaternions
            q1.Normalize();
            q2.Normalize();

            // Compute the cosine of the angle between the two quaternions
            float dot = q1.w * q2.w + q1.x * q2.x + q1.y * q2.y + q1.z * q2.z;

            // If the dot product is negative, negate one quaternion to take the shorter arc
            if (dot < 0.0f)
            {
                q2 = new Quaternion(-q2.w, -q2.x, -q2.y, -q2.z);
                dot = -dot;
            }

            const float DOT_THRESHOLD = 0.9995f;
            if (dot > DOT_THRESHOLD)
            {
                // If the quaternions are very close, use linear interpolation
                Quaternion result = new Quaternion(
                    q1.w + t * (q2.w - q1.w),
                    q1.x + t * (q2.x - q1.x),
                    q1.y + t * (q2.y - q1.y),
                    q1.z + t * (q2.z - q1.z)
                );
                result.Normalize();
                return result;
            }

            // Compute the angle between the quaternions
            float theta_0 = (float)Math.Acos(dot);
            float theta = theta_0 * t;

            // Compute the sin values
            float sin_theta = (float)Math.Sin(theta);
            float sin_theta_0 = (float)Math.Sin(theta_0);

            float s0 = (float)Math.Cos(theta) - dot * sin_theta / sin_theta_0;
            float s1 = sin_theta / sin_theta_0;

            return new Quaternion(
                s0 * q1.w + s1 * q2.w,
                s0 * q1.x + s1 * q2.x,
                s0 * q1.y + s1 * q2.y,
                s0 * q1.z + s1 * q2.z
            );
        }
        // 오일러 각 -> 쿼터니언 변환
        public static Quaternion FromEulerAngles(float roll, float pitch, float yaw)
        {
            roll = roll  * XMath.Deg2Rad;
            pitch = pitch * XMath.Deg2Rad;
            yaw = yaw * XMath.Deg2Rad;
            float cy = (float)Math.Cos(yaw * 0.5);
            float sy = (float)Math.Sin(yaw * 0.5);
            float cp = (float)Math.Cos(pitch * 0.5);
            float sp = (float)Math.Sin(pitch * 0.5);
            float cr = (float)Math.Cos(roll * 0.5);
            float sr = (float)Math.Sin(roll * 0.5);

            float w = cr * cp * cy + sr * sp * sy;
            float x = sr * cp * cy - cr * sp * sy;
            float y = cr * sp * cy + sr * cp * sy;
            float z = cr * cp * sy - sr * sp * cy;

            return new Quaternion(w, x, y, z);
        }

        // 쿼터니언 -> 오일러 각 변환
        public Vector3 ToEulerAngles()
        {
            float roll, pitch, yaw;

            // Roll (x-axis rotation)
            float sinr_cosp = 2 * (w * x + y * z);
            float cosr_cosp = 1 - 2 * (x * x + y * y);
            roll = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // Pitch (y-axis rotation)
            float sinp = 2 * (w * y - z * x);
            if (Math.Abs(sinp) >= 1)
                pitch = (float)Math.CopySign(Math.PI / 2, sinp); // Use 90 degrees if out of range
            else
                pitch = (float)Math.Asin(sinp);

            // Yaw (z-axis rotation)
            float siny_cosp = 2 * (w * z + x * y);
            float cosy_cosp = 1 - 2 * (y * y + z * z);
            yaw = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return new Vector3(roll, pitch, yaw) * XMath.Rad2Deg;
        }
    }
}
