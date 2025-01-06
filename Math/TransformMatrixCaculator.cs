using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Maths
{
    internal class TransformMatrixCaculator
    {
        public static Vector4 TransformH(Vector3 v, Matrix4x4 matrix)
        {
            Vector4 vector = new Vector4(v.x, v.y, v.z, 1);
            //동차좌표계에서 변환
            return new Vector4(
                vector.x * matrix.e11 + vector.y * matrix.e12 + vector.z * matrix.e13 + vector.w * matrix.e14,
                vector.x * matrix.e21 + vector.y * matrix.e22 + vector.z * matrix.e23 + vector.w * matrix.e24,
                vector.x * matrix.e31 + vector.y * matrix.e32 + vector.z * matrix.e33 + vector.w * matrix.e34,
                vector.x * matrix.e41 + vector.y * matrix.e42 + vector.z * matrix.e43 + vector.w * matrix.e44);
        }
        public static Vector3 Transform(Vector3 vector, Matrix4x4 matrix)
        {
            //동차좌표계에서 변환
            return new Vector3(
                vector.x * matrix.e11 + vector.y * matrix.e12 + vector.z * matrix.e13 + matrix.e14,
                vector.x * matrix.e21 + vector.y * matrix.e22 + vector.z * matrix.e23 + matrix.e24,
                vector.x * matrix.e31 + vector.y * matrix.e32 + vector.z * matrix.e33 + matrix.e34)
             / (vector.x * matrix.e41 + vector.y * matrix.e42 + vector.z * matrix.e43 + matrix.e44);
        }
        public static Matrix4x4 CreateObjectTransformMatrix(Vector3 position, Quaternion rot, Vector3 scale)
        {
            Vector3 rotation = rot.ToEulerAngles();
            rotation = rotation * XMath.Deg2Rad;
            float cx = (float)System.Math.Cos(rotation.x);
            float sx = (float)System.Math.Sin(rotation.x);
            float cy = (float)System.Math.Cos(rotation.y);
            float sy = (float)System.Math.Sin(rotation.y);
            float cz = (float)System.Math.Cos(rotation.z);
            float sz = (float)System.Math.Sin(rotation.z);
            return new Matrix4x4(
                    scale.x * (cz * cy), scale.y * (-sz * cx + cz * sy * sx), scale.z * (sz * sx + cz * sy * cx),  position.x,
                    scale.x * (sz * cy), scale.y * (cz * cx + sz * sy * sx),  scale.z * (-cz * sx + sz * sy * cx), position.y,
                    scale.x * (-sy),     scale.y * (cy * sx),                 scale.z * (cy * cx),                 position.z,
                    0f, 0f, 0f, 1f);
        }
        public static Matrix4x4 CreateRotationMatrix(Vector3 rotation)
        {
            rotation = rotation * XMath.Deg2Rad;
            float cx = (float)System.Math.Cos(rotation.x);
            float sx = (float)System.Math.Sin(rotation.x);
            float cy = (float)System.Math.Cos(rotation.y);
            float sy = (float)System.Math.Sin(rotation.y);
            float cz = (float)System.Math.Cos(rotation.z);
            float sz = (float)System.Math.Sin(rotation.z);
            return new Matrix4x4(
                    cz * cy, -sz * cx + cz * sy * sx, sz * sx + cz * sy * cx,  0f,                                    
                    sz * cy, cz * cx + sz * sy * sx,  -cz * sx + sz * sy * cx, 0f,                                     
                    -sy,     cy * sx,                 cy * cx,                 0f,                            
                    0f,       0f,                     0f,                      1f                                   
                );
            //            return new Matrix4x4(
            //    cy * cz, -cy * sz, sy, 0,
            //    cx * sz + sx * sy * cz, cx * cz - sx * sy * sz, -sx * cy, 0,
            //    sx * sz - cx * sy * cz, cx * sy * sz + sx * cz, cx * cy, 0,
            //    0, 0, 0, 1
            //);

            //Matrix4x4 rotationX = new Matrix4x4(
            //    1, 0, 0, 0,
            //    0, cx, -sx, 0,
            //    0, sx, cx, 0,
            //    0, 0, 0, 1);

            //Matrix4x4 rotationY = new Matrix4x4(
            //    cy, 0, sy, 0,
            //    0, 1, 0, 0,
            //    -sy, 0, cy, 0,
            //    0, 0, 0, 1);

            //Matrix4x4 rotationZ = new Matrix4x4(
            //    cz, -sz, 0, 0,
            //    sz, cz, 0, 0,
            //    0, 0, 1, 0,
            //    0, 0, 0, 1);

            //return rotationZ * rotationY * rotationX;
        }

        public static Matrix4x4 CreateTranslationMatrix(Vector3 translation)
        {
            return new Matrix4x4(
                1, 0, 0, translation.x,
                0, 1, 0, translation.y,
                0, 0, 1, translation.z,
                0, 0, 0, 1);
        }
    }
}
