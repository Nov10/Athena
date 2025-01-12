using Athena.Maths;
using Athena.Engine.Core.Rendering;
using System.Collections.Generic;
using Renderer.Engine.Core.Image;
using Renderer.Engine.Core.Rendering;

namespace Athena.Engine.Core
{
    public class Camera : Core.Component
    {
        public static List<Camera> CameraList = new List<Camera>();
        public BaseRenderer MainRenderer;
        public RenderBitmap RenderTarget { get; private set; }
        public void SetRenderTarget(RenderBitmap renderTarget)
        {
            RenderTarget = renderTarget;
        }
        public Camera()
        {
            CameraList.Add(this);
        }
        ~Camera()
        {
            CameraList.Remove(this);
        }
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
            f4 = -2 * FarPlaneDistance * NearPlaneDistance / (FarPlaneDistance - NearPlaneDistance);
        }

        public Matrix4x4 CalculateRenderMatrix()
        {
            //Vector3 zAxis = Controller.WorldRotation.RotateVector(new Vector3(0, 0, 1));
            Vector3 zAxis = Controller.Forward;
            Vector3 xAxis = Controller.Right;
            Vector3 yAxis = Controller.Up;
            Vector3 t = -Controller.WorldPosition;

            return new Matrix4x4(
            f1 * xAxis.x, f1 * xAxis.y, f1 * xAxis.z, f1 * Vector3.Dot(xAxis, t),
            f2 * yAxis.x, f2 * yAxis.y, f2 * yAxis.z, f2 * Vector3.Dot(yAxis, t),
            f3 * zAxis.x, f3 * zAxis.y, f3 * zAxis.z, f3 * Vector3.Dot(zAxis, t) + f4,
            zAxis.x, zAxis.y, zAxis.z, Vector3.Dot(zAxis, t));
        }

        public override void Start()
        {
            CalculatePerspectiveMatrixValues();
        }

        public override void Update()
        {
        }

        public void Render(List<MeshRenderer> objects)
        {
            if (MainRenderer == null)
                return;

            MainRenderer.Render(this, objects);
        }

        public override void Awake()
        {

        }
    }
}
