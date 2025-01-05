using Renderer.Maths;
using Renderer.Renderer.PBR;
using System.Collections.Generic;

namespace Renderer.Renderer
{
    public class Camera : Core.Component
    {
        float _NearPlaneDistance;
        float _FarPlaneDistance;
        float _FieldOfView;
        float _AspectRatio;
        public float NearPlaneDistance
        {
            get { return _NearPlaneDistance; }
            set
            {
                _NearPlaneDistance = value;
                CalculatePerspectiveMatrixValues();
            }
        }
        public float FarPlaneDistance
        {
            get { return _FarPlaneDistance; }
            set
            {
                _FarPlaneDistance = value;
                CalculatePerspectiveMatrixValues();
            }
        }
        public float FieldOfView
        {
            get { return _FieldOfView; }
            set
            {
                _FieldOfView = value;
                CalculatePerspectiveMatrixValues();
            }
        }
        public float AspectRatio
        {
            get { return _AspectRatio; }
            set
            {
                _AspectRatio = value;
                CalculatePerspectiveMatrixValues();
            }
        }
        public Vector3 WorldUp = new Vector3(0, 1, 0); // 월드좌표계의 위쪽

        public PBRRenderer MainRenderer;
        float distance = 1;
        float height;
        float width;
        float f1;
        float f2;
        float f3;
        float f4;
        float f5;
        void CalculatePerspectiveMatrixValues()
        {
            height = (float)System.Math.Tan(FieldOfView * XMath.Deg2Rad_Half) * distance * 2;
            width = height * AspectRatio;
            f1 = NearPlaneDistance / width;
            f2 = NearPlaneDistance / height;
            f3 = (FarPlaneDistance + NearPlaneDistance) / (FarPlaneDistance - NearPlaneDistance);
            f4 = 2 * FarPlaneDistance * NearPlaneDistance / (FarPlaneDistance - NearPlaneDistance);
            //f5 = -1;
            ////f1 = NearPlaneDistance / width;
            ////f2 = NearPlaneDistance / height;
            ////f3 = -1;
            //////f3 = (FarPlaneDistance + NearPlaneDistance) / (FarPlaneDistance - NearPlaneDistance);
            //////f4 = 2* FarPlaneDistance * NearPlaneDistance / (FarPlaneDistance - NearPlaneDistance);
            ////f4 = -2 * NearPlaneDistance;
            ////f5 = -1;
        }

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


        //public Matrix4x4 CalculatePerspectiveProjectionMatrix()
        //{
        //    return CalculateRenderMatrix();
        //    //return CalculateProjectionMatrix() * CalculateCameraTransformMatrix();
        //}

        public Matrix4x4 CalculateRenderMatrix()
        {

            Vector3 zAxis = Controller.WorldRotation.RotateVector(new Vector3(0, 0, 1));
            Vector3 xAxis = (Vector3.Cross(WorldUp, zAxis)).normalized;
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis).normalized;
            Vector3 t = -Controller.WorldPosition;

            return new Matrix4x4(
            f1 * xAxis.x, f1 * xAxis.y, f1 * xAxis.z, f1 * Vector3.Dot(xAxis, t),
            f2 * yAxis.x, f2 * yAxis.y, f2 * yAxis.z, f2 * Vector3.Dot(yAxis, t),
            f3 * zAxis.x, f3 * zAxis.y, f3 * zAxis.z, (f3 * Vector3.Dot(zAxis, t) - f4),
            zAxis.x, zAxis.y, zAxis.z, Vector3.Dot(zAxis, t));
        }


        //public Matrix4x4 CalculateProjectionMatrix()
        //{
        //    float distance = 1;
        //    float height = (float)System.Math.Tan(FieldOfView * System.Math.PI / 180 / 2) * distance * 2;
        //    float width = height * AspectRatio;
        //    float f = FarPlaneDistance;
        //    float n = NearPlaneDistance;


        //    return new Matrix4x4(
        //     2 * distance / width, 0, 0, 0,
        //     0, 2 * distance / height, 0, 0,
        //     0, 0, -(f + n) / (f - n), -2 * f * n / (f - n),
        //     0, 0, -1, 0);
        //}

        //public Matrix4x4 CalculateCameraTransformMatrix()
        //{
        //    Vector3 zAxis = Controller.WorldRotation.RotateVector(new Vector3(0, 0, -1));
        //    Vector3 xAxis = (Vector3.Cross(WorldUp, zAxis)).normalized;
        //    Vector3 yAxis = Vector3.Cross(zAxis, xAxis).normalized;

        //    return new Matrix4x4(
        //        xAxis.x, xAxis.y, xAxis.z, Vector3.Dot(xAxis, -Controller.WorldPosition),
        //        yAxis.x, yAxis.y, yAxis.z, Vector3.Dot(yAxis, -Controller.WorldPosition),
        //        -zAxis.x, -zAxis.y, -zAxis.z, Vector3.Dot(-zAxis, -Controller.WorldPosition),
        //        0, 0, 0, 1);
        //    //아래와 동일

        //    //Matrix4x4 rotationMatrix = new Matrix4x4(
        //    //    xAxis.x, yAxis.x, -zAxis.x, 0,
        //    //    xAxis.y, yAxis.y, -zAxis.y, 0,
        //    //    xAxis.z, yAxis.z, -zAxis.z, 0,
        //    //    0, 0, 0, 1);

        //    //Matrix4x4 translationMatrix = new Matrix4x4(
        //    //    1, 0, 0, -Controller.WorldPosition.x,
        //    //    0, 1, 0, -Controller.WorldPosition.y,
        //    //    0, 0, 1, -Controller.WorldPosition.z,
        //    //    0, 0, 0, 1);

        //    //return rotationMatrix.Transpose() * translationMatrix;
        //}

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

        public override void Awake()
        {
        }
    }
}
