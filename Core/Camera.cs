using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Renderer
{
    public class Camera
    {
        public Vector3 Position;
        //public Quaternion Rotation;
        public Vector3 Direction;

        public float NearPlaneDistance;
        public float FarPlaneDistance;
        public float FieldOfView;
        public float AspectRatio;
        public Vector3 WorldUp = new Vector3(0, 1, 0); // 월드좌표계의 위쪽

        //public Camera(Vector3 position, Vector3 direction, float nearPlaneDistance, float farPlaneDistance, float fieldOfView, float aspectRatio, Vector3 worldUp)
        //{
        //    Position = position;
        //    Direction = direction;
        //    NearPlaneDistance = nearPlaneDistance;
        //    FarPlaneDistance = farPlaneDistance;
        //    FieldOfView = fieldOfView;
        //    AspectRatio = aspectRatio;
        //    WorldUp = worldUp;
        //}


        public Matrix4x4 CalculatePerspectiveProjectionMatrix()
        {
            return CalculateProjectionMatrix() * CalculatePerspectiveMatrix();
        }

        public Matrix4x4 CalculateProjectionMatrix()
        {
            float distance = 0.1f;
            float height = (float)System.Math.Tan(FieldOfView * System.Math.PI / 180 / 2) * distance * 2;
            float width = height * AspectRatio;
            float f = FarPlaneDistance;
            float n = NearPlaneDistance;


            return new Matrix4x4(
             2 * distance / width, 0, 0, 0,
             0, 2 * distance / height, 0, 0,
             0, 0, -(f + n) / (f - n), -2 * f * n / (f - n),
             0, 0, -1, 0);
        }
        public Matrix4x4 CalculateRotationMatrix()
        {
            Vector3 zAxis = Direction.normalized;
            Vector3 xAxis = (Vector3.Cross(WorldUp, zAxis)).normalized;
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis).normalized;

            Matrix4x4 rotationMatrix = new Matrix4x4(
                xAxis.x, yAxis.x, -zAxis.x, 0,
                xAxis.y, yAxis.y, -zAxis.y, 0,
                xAxis.z, yAxis.z, -zAxis.z, 0,
                0, 0, 0, 1);
            return rotationMatrix.Inverse();
        }
        public Matrix4x4 CalculatePerspectiveMatrix()
        {
            Vector3 zAxis = Direction.normalized;
            Vector3 xAxis = (Vector3.Cross(WorldUp, zAxis)).normalized;
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis).normalized;

            Matrix4x4 rotationMatrix = new Matrix4x4(
                xAxis.x, yAxis.x, -zAxis.x, 0,
                xAxis.y, yAxis.y, -zAxis.y, 0,
                xAxis.z, yAxis.z, -zAxis.z, 0,
                0, 0, 0, 1);

            Matrix4x4 translationMatrix = new Matrix4x4(
                1, 0, 0, -Position.x,
                0, 1, 0, -Position.y,
                0, 0, 1, -Position.z,
                0, 0, 0, 1);

            return rotationMatrix.Inverse() * translationMatrix;
        }
    }
}
