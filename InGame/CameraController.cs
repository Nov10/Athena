using Renderer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renderer.Maths;
using Windows.Devices.Pwm;

namespace Renderer.InGame
{
    public class CameraController : Component
    {
        public Core.Object Target;
        public override void Awake()
        {
        }

        public override void Start()
        {
        }

        public override void Update()
        {
            //Controller.WorldPosition = Vector3.Lerp(Controller.WorldPosition, Target.WorldPosition + -Target.Forward * 15, 0.5f);
            //Controller.WorldRotation = Quaternion.Slerp(Controller.WorldRotation, Target.WorldRotation, 0.5f).Normalize();
        }
    }
}
