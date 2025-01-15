using Athena.Maths;
using Athena.Engine.Core.Rendering;
using System.Collections.Generic;
using Athena.Engine.Core.Image;

namespace Athena.Engine.Core
{
    public class Camera : Core.Component
    {
        public RenderBitmap RenderTarget { get; private set; }
        public void SetRenderTarget(RenderBitmap renderTarget)
        {
            RenderTarget = renderTarget;
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
                CalculateVPMatrixValues();
            }
        }
        public float FarPlaneDistance
        {
            get { return _FarPlaneDistance; }
            set
            {
                _FarPlaneDistance = value;
                CalculateVPMatrixValues();
            }
        }
        public float FieldOfView
        {
            get { return _FieldOfView; }
            set
            {
                _FieldOfView = value;
                CalculateVPMatrixValues();
            }
        }
        public float AspectRatio
        {
            get { return _AspectRatio; }
            set
            {
                _AspectRatio = value;
                CalculateVPMatrixValues();
            }
        }


        float distance = 1;
        float height;
        float width;
        float f1;
        float f2;
        float f3;
        float f4;
        void CalculateVPMatrixValues()
        {
            height = (float)System.Math.Tan(FieldOfView * XMath.Deg2Rad_Half) * distance * 2;
            width = height * AspectRatio;
            f1 = NearPlaneDistance / width;
            f2 = NearPlaneDistance / height;
            f3 = (FarPlaneDistance + NearPlaneDistance) / (FarPlaneDistance - NearPlaneDistance);
            f4 = -2 * FarPlaneDistance * NearPlaneDistance / (FarPlaneDistance - NearPlaneDistance);
        }

        public Matrix4x4 CalculateVPMatrix()
        {
            Vector3 zAxis, yAxis, xAxis;
            (zAxis, xAxis, yAxis) = Controller.GetDirections();
            Vector3 t = -Controller.WorldPosition;

            return new Matrix4x4(
            f1 * xAxis.x, f1 * xAxis.y, f1 * xAxis.z, f1 * Vector3.Dot(xAxis, t),
            f2 * yAxis.x, f2 * yAxis.y, f2 * yAxis.z, f2 * Vector3.Dot(yAxis, t),
            f3 * zAxis.x, f3 * zAxis.y, f3 * zAxis.z, f3 * Vector3.Dot(zAxis, t) + f4,
            zAxis.x, zAxis.y, zAxis.z, Vector3.Dot(zAxis, t));
        }

        public override void Start()
        {
            CalculateVPMatrixValues();
        }

        public override void Update()
        {
        }

        public override void Awake()
        {
        }
    }
}
