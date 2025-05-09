﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Maths
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
        public Quaternion(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public readonly float magnitude
        {
            get { return (float)System.MathF.Sqrt(sqrMagnitude); }
        }
        public readonly float sqrMagnitude
        {
            get { return x * x + y * y + z * z + w * w; }
        }
        public readonly Quaternion normalized
        {
            get { return Scale(this, 1/magnitude); }
        }

        public Quaternion Inverse()
        {
            return new Quaternion(w, -x, -y, -z).normalized;
        }
        public Quaternion Conjugate()
        {
            return new Quaternion(w, -x, -y, -z);
        }
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


        public static Quaternion Scale(Quaternion q, float scale)
        {
            return new Quaternion(q.w * scale, q.x * scale, q.y * scale, q.z * scale);
        }
        public static Quaternion operator *(Quaternion q1, Quaternion q2)
        {
            float nw = q1.w * q2.w - q1.x * q2.x - q1.y * q2.y - q1.z * q2.z;
            float nx = q1.w * q2.x + q1.x * q2.w + q1.y * q2.z - q1.z * q2.y;
            float ny = q1.w * q2.y - q1.x * q2.z + q1.y * q2.w + q1.z * q2.x;
            float nz = q1.w * q2.z + q1.x * q2.y - q1.y * q2.x + q1.z * q2.w;
            return new Quaternion(nw, nx, ny, nz);
        }


        public Vector3 RotateVector(Vector3 v)
        {
            Quaternion qVec = new Quaternion(0, v.x, v.y, v.z);
            Quaternion qConj = this.Conjugate();
            Quaternion qRes = this * qVec * qConj;
            return new Vector3( qRes.x, qRes.y, qRes.z );
        }

        /// <summary>
        /// (0, 0, 1)을 회전시킵니다. 쿼터니언 곱을 줄이기 위해 사용합니다.
        /// </summary>
        public Vector3 RotateVectorZDirection()
        {
            var q= new Quaternion(-z, y, -x, w) * this.Conjugate();
            return new Vector3(q.x, q.y, q.z);
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
        public static Quaternion LookDirection(Vector3 forward, Vector3? up = null)
        {
            // up 벡터가 지정되지 않았다면 기본값 (0,1,0) 사용
            Vector3 defaultUp = up ?? new Vector3(0, 1, 0);

            // forward, up 둘 다 0 벡터인 경우 예외처리
            if (forward.sqrMagnitude < 1e-12f)
            {
                // forward가 거의 0벡터라면 '단위 쿼터니언'을 반환하거나,
                // 혹은 원하는 기본값을 반환하면 됩니다.
                return new Quaternion(1, 0, 0, 0);
            }

            // forward 정규화
            forward = forward.normalized;

            // up 정규화
            defaultUp = defaultUp.normalized;

            // forward와 up 이 거의 평행이라면(크로스가 0에 가까우면), 
            // 적절히 보정하는 로직이 필요할 수 있음.
            // 여기서는 단순 처리.
            Vector3 right = Vector3.Cross(defaultUp, forward);
            if (right.sqrMagnitude < 1e-12f)
            {
                // up 이 forward와 평행하다면 보정
                // 임의의 축을 하나 잡아서 교차곱으로 축을 만든 뒤 up을 다시 구함
                // 예: forward와 가장 덜 평행한 축을 찾아서 cross 연산
                // 아래는 간단 예시
                Vector3 axis = Math.Abs(forward.x) > Math.Abs(forward.z)
                    ? new Vector3(0, 0, 1)
                    : new Vector3(1, 0, 0);
                right = Vector3.Cross(axis, forward).normalized;
            }
            else
            {
                // 평행하지 않다면 정규화
                right = right.normalized;
            }

            // 재계산된 up
            Vector3 recalculatedUp = Vector3.Cross(forward, right);

            // 이제 3x3 회전 행렬( row-major )을 구성합니다.
            //   [ right.x   right.y   right.z  ]
            //   [ up.x      up.y      up.z     ]
            //   [ forward.x forward.y forward.z]
            //
            // 유니티 관점에서 
            //  X축 = right
            //  Y축 = up
            //  Z축 = forward
            //
            // row-major 순서로 m00 = right.x, m01 = right.y, m02 = right.z ...
            // (주의) 행렬에서 어떤 축을 행/열 중 어디에 배치하느냐에 따라 공식이 달라집니다.
            float m00 = right.x; float m01 = right.y; float m02 = right.z;
            float m10 = recalculatedUp.x;
            float m11 = recalculatedUp.y;
            float m12 = recalculatedUp.z;
            float m20 = forward.x; float m21 = forward.y; float m22 = forward.z;

            // 행렬 -> 쿼터니언 변환
            float trace = m00 + m11 + m22;
            float qw, qx, qy, qz;

            if (trace > 0.0f)
            {
                float s = (float)Math.Sqrt(trace + 1.0f) * 2f;
                qw = 0.25f * s;
                qx = (m12 - m21) / s;
                qy = (m20 - m02) / s;
                qz = (m01 - m10) / s;
            }
            else if ((m00 > m11) && (m00 > m22))
            {
                float s = (float)Math.Sqrt(1.0f + m00 - m11 - m22) * 2f;
                qw = (m12 - m21) / s;
                qx = 0.25f * s;
                qy = (m10 + m01) / s;
                qz = (m20 + m02) / s;
            }
            else if (m11 > m22)
            {
                float s = (float)Math.Sqrt(1.0f + m11 - m00 - m22) * 2f;
                qw = (m20 - m02) / s;
                qx = (m10 + m01) / s;
                qy = 0.25f * s;
                qz = (m21 + m12) / s;
            }
            else
            {
                float s = (float)Math.Sqrt(1.0f + m22 - m00 - m11) * 2f;
                qw = (m01 - m10) / s;
                qx = (m20 + m02) / s;
                qy = (m21 + m12) / s;
                qz = 0.25f * s;
            }

            var q = new Quaternion(qw, qx, qy, qz);

            // 쿼터니언 정규화
            return q.normalized;
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
            return result.normalized;
        }
        public static Quaternion Slerp(Quaternion q1, Quaternion q2, float t)
        {
            if (t < 0)
                t = 0;
            else if (t > 1)
                t = 1;
            //if (t < 0 || t > 1)
            //    throw new ArgumentOutOfRangeException(nameof(t), "t must be in the range [0, 1].");

            // Normalize the quaternions
            q1 = q1.normalized;
            q2 = q2.normalized;

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
                return result.normalized;
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
