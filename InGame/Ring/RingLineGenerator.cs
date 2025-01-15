using Athena.Engine.Core;
using Athena.Engine.Core.Rendering;
using Athena.Engine.Core.Rendering.Shaders;
using Athena.InGame.Ring;
using Athena.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Engine.Core.Image;

namespace Athena.InGame.Ring
{
    public class RingLineGenerator : Component
    {
        float RangeStart = 5;
        float RangeEnd = 1000;

        Vector3 Function(float t)
        {
            return new Vector3(t*t*0.01f + MathF.Sin(t/10f)*15, MathF.Cos(t / 10f) *10 + 10, t + MathF.Sin(t / 10f) *9);
        }
        float RadiusFunction(float t)
        {
            return 8;
        }

        float Interval = 20;
        const float Delta = 0.1f;
        Vector3 PositionBias = new Vector3(0, 20, 0);
        CircleRing[] Rings;
        public CircleRing[] Generate()
        {
            var lineRenderData = RenderData.CreateExtrudedLineFromFunction(Function, RangeStart, RangeEnd, 300, 0.2f, 0.2f, Color.white); ;
            lineRenderData.Shader = new UnlitColorShader(Color.black);
            MeshRenderer m = new MeshRenderer();
            m.RenderDatas.Add(lineRenderData);
            GameObject g = new GameObject();
            g.WorldPosition = PositionBias;
            g.AddComponent(m);

            List<CircleRing> rings = new List<CircleRing>();
            for (float t = RangeStart; t <= RangeEnd; t += Interval)
            {
                Vector3 position = Function(t) + PositionBias;
                Vector3 position_delta = Function(t - 0.1f) + PositionBias;

                var ring = CircleRing.CreateRingObject(RadiusFunction(t));
                ring.Controller.WorldPosition = position;
                ring.Controller.WorldRotation = Quaternion.LookDirection((position_delta - position));
                rings.Add(ring);
            }
            return rings.ToArray();
        }

        public override void Awake()
        {
        }

        public override void Start()
        {
            Rings = Generate();
            Rings[NowRingIndex].ShouldCheck = true;
        }
        int NowRingIndex = 0;
        float ElapsedTime = 0.0f;
        public override void Update()
        {
            if (Rings[NowRingIndex].IsFailed == true)
            {
                Rings[NowRingIndex].ShouldCheck = false;
                NowRingIndex++;
                Rings[NowRingIndex].ShouldCheck = true;
            }

            else if (Rings[NowRingIndex].IsPassed == false)
            {
                ElapsedTime += Time.DeltaTime;
                //System.Diagnostics.Debug.WriteLine(ElapsedTime);
                if (ElapsedTime > 100f)
                {
                    Rings[NowRingIndex].ShouldCheck = false;
                    Rings[NowRingIndex].SetColor(new Color(255, 0, 0, 255));
                    NowRingIndex++;
                    Rings[NowRingIndex].ShouldCheck = true;
                    ElapsedTime = 0.0f;
                }
            }
            else
            {
                Rings[NowRingIndex].ShouldCheck = false;
                NowRingIndex++;
                Rings[NowRingIndex].ShouldCheck = true;
            }
        }
    }
}
