using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;

namespace Athena.Engine.Core.Rendering.Lights
{
    public struct LightData
    {
        public float Intensity;
        public Vector3 Direction;
    }
    public abstract class Light : Component
    {
        public LightType Type;
        public LightData Data;

        public static List<Light> Lights = new List<Light>();

        public void UpdateLightData()
        {
            Data.Direction = Controller.WorldRotation.RotateVectorZDirection();
        }

        public override void Start()
        {
        }
        public override void Awake()
        {
            Lights.Add(this);
        }
        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            Lights.Remove(this);
        }
    }
}
