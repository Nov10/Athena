﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Maths
{
    public class Quaternion
    {
        public float x; public float y; public float z; public float w;

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
    }
}