using Athena.Engine.Core.Rendering;
using Athena.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using Athena.Engine.Core.Image;

namespace Renderer.InGame.Ring
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
                Controller.LocalScale = new Vector3(Radius  / 7.5f, Radius / 7.5f, 1);
            }
        }

        public const float AllowDistanceZ = 2;

        bool IsFrontPassed;
        bool IsBackPassed;
        bool IsPassed { get { return IsFrontPassed && IsBackPassed; } }
        bool IsObjectPassedFrontPoint(GameObject gameObject)
        {
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

        public static CircleRing CreateRingObject(float radius)
        {
            var renderer = Athena.MeshLoader.FBXLoader.LoadFBX_SeperatedAsRenderer(@"C:\ring.fbx");
            GameObject torous = new GameObject();
            Athena.Engine.Core.MeshRenderer torousRenderer = new Athena.Engine.Core.MeshRenderer();
            torousRenderer.RenderDatas.Add(renderer.RenderDatas[0]);
            torousRenderer.RenderDatas[0].Shader = new SimpleColorShader(new Color(255, 255, 255, 255));
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
        }
        float FrontPassedTime;
        float BackPassedTime;
        public override void Update()
        {
            if (IsPassed == true)
                return;
            GameObject player = GameFlowManager.Instance.Player.Controller;

            if(IsFrontPassed == false)
            {
                bool passed = IsObjectPassedFrontPoint(player);
                IsFrontPassed = passed;
                if (passed)
                {
                    FrontPassedTime = Time.TotalTime;
                    System.Diagnostics.Debug.WriteLine("Passed Front");
                    if (Controller.TryGetComponent(out MeshRenderer renderer))
                    {
                        renderer.RenderDatas[0].Shader = new SimpleColorShader(new Color(255, 0, 0, 255));
                    }
                }
            }

            if(IsFrontPassed == true && IsBackPassed == false)
            {
                bool passed = IsObjectPassedBackPoint(player);
                IsBackPassed = passed;
                if (passed)
                {
                    BackPassedTime = Time.TotalTime;
                    System.Diagnostics.Debug.WriteLine("Passed Back");
                }
            }

            if(IsFrontPassed == true && IsBackPassed == false)
            {
                float deltaTime = Time.TotalTime - FrontPassedTime;
                if(deltaTime > 3f)
                {
                    IsFrontPassed = false;
                    if (Controller.TryGetComponent(out MeshRenderer renderer))
                    {
                        renderer.RenderDatas[0].Shader = new SimpleColorShader(new Color(255, 255, 255, 255));
                    }
                }
            }

            if(IsPassed == true)
            {
                GameFlowManager.Instance.AddScore();
                if(Controller.TryGetComponent(out MeshRenderer renderer))
                {
                    renderer.RenderDatas[0].Shader = new SimpleColorShader(new Color(0, 0, 0, 255));
                }
            }
        }
    }
}
