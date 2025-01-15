using Athena.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using Athena.Engine.Core.Image;
using Athena.Engine.Core.Rendering.Shaders;
using System.Reflection;

namespace Athena.InGame.Ring
{
    public class CircleRing : Component
    {
        float _Radius = 1;
        public float Radius
        {
            get { return _Radius; }
            set
            {
                _Radius = value;
                Controller.LocalScale = new Vector3(Radius  / 3.5f, Radius / 3.5f, 1);
            }
        }
        UnlitColorShader ColorShader;

        public const float AllowDistanceZ = 2;

        bool IsFrontPassed;
        bool IsBackPassed;
         public bool IsPassed { get { return IsFrontPassed && IsBackPassed; } }
        bool IsObjectPassedFrontPoint(GameObject gameObject)
        {
            if (Vector3.Dot(gameObject.Forward, Controller.Forward) > 0)
                return false;

            Matrix4x4 inverseTransformMatrix = TransformMatrixCaculator.CreateObjectPosInvRotMatrix(-Controller.WorldPosition, Controller.WorldRotation);
            Vector4 p = TransformMatrixCaculator.TransformH(gameObject.WorldPosition, inverseTransformMatrix);
            float w = p.w;
            if (0 <= p.z && p.z <= AllowDistanceZ * w)
            {
                if ((p.x * p.x + p.y * p.y) <= Radius * Radius * w * w)
                    return true;
            }
            return false;
        }

        bool IsObjectPassedBackPoint(GameObject gameObject)
        {
            if (Vector3.Dot(gameObject.Forward, Controller.Forward) > 0)
                return false;
            Matrix4x4 inverseTransformMatrix = TransformMatrixCaculator.CreateObjectPosInvRotMatrix(-Controller.WorldPosition, Controller.WorldRotation);
            Vector4 p = TransformMatrixCaculator.TransformH(gameObject.WorldPosition, inverseTransformMatrix);
            float w = p.w;
            if (-AllowDistanceZ * w <= p.z && p.z <= 0)
            {
                if ((p.x * p.x + p.y * p.y) <= Radius * Radius * w * w)
                    return true;
            }
            return false;
        }
        bool IsObjectPassFailed(GameObject gameObject)
        {
            if (Vector3.Dot(gameObject.Forward, Controller.Forward) > 0)
                return false;
            Matrix4x4 inverseTransformMatrix = TransformMatrixCaculator.CreateObjectPosInvRotMatrix(-Controller.WorldPosition, Controller.WorldRotation);
            Vector4 p = TransformMatrixCaculator.TransformH(gameObject.WorldPosition, inverseTransformMatrix);
            float w = p.w;
            if (-AllowDistanceZ * w <= p.z && p.z <= 0)
            {
                return true;
            }
            return false;
        }
        public static CircleRing CreateRingObject(float radius)
        {
            var renderer = Athena.MeshLoader.FBXLoader.LoadFBX_SeperatedAsRenderer("ring.fbx");
            GameObject torous = new GameObject();
            Athena.Engine.Core.MeshRenderer torousRenderer = new Athena.Engine.Core.MeshRenderer();
            torousRenderer.RenderDatas.Add(renderer.RenderDatas[0]);
            torousRenderer.RenderDatas[0].Shader = new UnlitColorShader(new Color(255, 255, 255, 255));
            torous.WorldPosition = new Vector3(0, 50, 5);
            torous.AddComponent(torousRenderer);

            CircleRing ring = new CircleRing();
            torous.AddComponent(ring);
            ring.Radius = radius;

            return ring;
        }

        public override void Awake()
        {
        }

        public override void Start()
        {
            if(Controller.TryGetComponent(out MeshRenderer renderer))
            {
                ColorShader = renderer.RenderDatas[0].Shader as UnlitColorShader;
            }
        }
        public void SetColor(Color c)
        {
            ColorShader.SetColor(c);
        }
        float FrontPassedTime;
        float BackPassedTime;

        public bool ShouldCheck = false;
        public bool IsFailed = false;
        public override void Update()
        {
            if (ShouldCheck == false)
                return;
            if (IsPassed == true || IsFailed == true)
                return;
            GameObject player = GameFlowManager.Instance.Player.Controller;

            if(IsFrontPassed == false)
            {
                bool passed = IsObjectPassedFrontPoint(player);
                IsFrontPassed = passed;
                if (passed)
                {
                    FrontPassedTime = Time.TotalTime;
                    ColorShader.SetColor(new Color(0, 0, 255, 255));
                }

                if(IsObjectPassFailed(player))
                {
                    IsFailed = true;
                    ColorShader.SetColor(new Color(255, 0, 0, 255));
                }
            }

            if(IsFrontPassed == true && IsBackPassed == false)
            {
                bool passed = IsObjectPassedBackPoint(player);
                IsBackPassed = passed;
                if (passed)
                {
                    BackPassedTime = Time.TotalTime;
                }
            }

            if(IsFrontPassed == true && IsBackPassed == false)
            {
                float deltaTime = Time.TotalTime - FrontPassedTime;
                if(deltaTime > 3f)
                {
                    IsFrontPassed = false;
                    ColorShader.SetColor(new Color(255, 255, 255, 255));
                }
            }

            if(IsPassed == true)
            {
                GameFlowManager.Instance.AddScore();
                ColorShader.SetColor(new Color(0, 255, 0, 255));
            }
        }
    }
}
