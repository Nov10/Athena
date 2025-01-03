using Renderer.Maths;
using Renderer.Renderer.PBR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Renderer
{
    public class Camera : Core.Component
    {
        //public Quaternion Rotation;
        //public Vector3 Direction;

        public float NearPlaneDistance;
        public float FarPlaneDistance;
        public float FieldOfView;
        public float AspectRatio;
        public Vector3 WorldUp = new Vector3(0, 1, 0); // 월드좌표계의 위쪽

        public PBRRenderer MainRenderer;

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
            float distance = 1;
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
            Vector3 zAxis = Controller.WorldRotation.RotateVector(new Vector3(0, 0, -1));
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
            Vector3 zAxis = Controller.WorldRotation.RotateVector(new Vector3(0, 0, -1));
            Vector3 xAxis = (Vector3.Cross(WorldUp, zAxis)).normalized;
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis).normalized;

            Matrix4x4 rotationMatrix = new Matrix4x4(
                xAxis.x, yAxis.x, -zAxis.x, 0,
                xAxis.y, yAxis.y, -zAxis.y, 0,
                xAxis.z, yAxis.z, -zAxis.z, 0,
                0, 0, 0, 1);

            Matrix4x4 translationMatrix = new Matrix4x4(
                1, 0, 0, -Controller.WorldPosition.x,
                0, 1, 0, -Controller.WorldPosition.y,
                0, 0, 1, -Controller.WorldPosition.z,
                0, 0, 0, 1);

            return rotationMatrix.Inverse() * translationMatrix;
        }

        public override void Start()
        {
        }

        public override void Update()
        {
        }

        public void Render(List<Core.Object> objects)
        {
            MainRenderer.camera = this;
            for (int i = 0; i < objects.Count; i++)
            {
                if(objects[i].TryGetComponent(out Core.Renderer ren))
                {
                    MainRenderer.AddObject(ren);
                }
            }
            MainRenderer.Render();

            MainRenderer.Targets.Clear();
        }
    }
}
