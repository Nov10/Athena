using Athena.Maths;
using Athena.Engine.Core.Rendering;
using System.Collections.Generic;
using Athena.Engine.Core.Image;
using Microsoft.UI.Xaml.Controls;

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
        public Matrix4x4 CalculateVPMatrix_Orthographic()
        {
            Vector3 zAxis, yAxis, xAxis;
            (zAxis, xAxis, yAxis) = Controller.GetDirections();
            Vector3 t = -Controller.WorldPosition;
            NearPlaneDistance = 0;
            // 2. 직교 투영 행렬(Projection) 계산
            float invRL = 1.0f / 256;
            float invTB = 1.0f / 256;
            float invFN = 1.0f / (FarPlaneDistance - NearPlaneDistance);
            Matrix4x4 ortho = new Matrix4x4(
            2f * invRL, 0f, 0f, 0,
                0f, 2f * invTB, 0f, 0,
                0f, 0f, invFN, -(NearPlaneDistance) * invFN,
                0f, 0f, 0f, 1f
            );
            return ortho * new Matrix4x4(
            xAxis.x, xAxis.y, xAxis.z, Vector3.Dot(xAxis, t),
            yAxis.x, yAxis.y, yAxis.z, Vector3.Dot(yAxis, t),
            zAxis.x, zAxis.y, zAxis.z, Vector3.Dot(zAxis, t),
            0, 0, 0, 1);
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
