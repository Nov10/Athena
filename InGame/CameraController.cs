using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using Windows.Devices.Pwm;
using Athena.Engine.Core;

namespace Athena.InGame
{
    public class CameraController : Component
    {
        public GameObject Target;
        public override void Awake()
        {
        }

        public override void Start()
        {
        }
        public float MoveSpeed;
        public float RotateSpeed;

        public override void Update()
        {
            var moveInput = Input.GetDirectionInput2D(KeyPreset.WASD);
            //Input.DebugNowInputKeys();
            Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

            if (Input.GetKey(KeyCode.Q))
                move.y = 1;
            else if (Input.GetKey(KeyCode.E))
                move.y = -1;
            ////System.Diagnostics.Debug.WriteLine(WorldObjects[3].WorldPosition);
            ///
            //Controller.WorldPosition += move;

            var rotateInput = Input.GetDirectionInput2D(KeyPreset.Arrow);
            Quaternion q;
            //System.Diagnostics.Debug.WriteLine(WorldObjects[3].WorldPosition);
            if (rotateInput.y > 0.5f)
                q = Quaternion.CreateRotationQuaternion(new Vector3(1, 0, 0), -5);
            else if (rotateInput.y < -0.5f)
                q = Quaternion.CreateRotationQuaternion(new Vector3(1, 0, 0), 5);
            else if (rotateInput.x > 0.5f)
                q = Quaternion.CreateRotationQuaternion(new Vector3(0, 1, 0), -5);
            else if (rotateInput.x < -0.5f)
                q = Quaternion.CreateRotationQuaternion(new Vector3(0, 1, 0), 5);
            else
                q = new Quaternion(1, 0, 0, 0);
            Controller.WorldRotation = Controller.WorldRotation * q;
            Controller.WorldPosition = Vector3.Lerp(Controller.WorldPosition, Target.WorldPosition + -Target.Forward * 15, Time.DeltaTime * MoveSpeed);
            Controller.WorldRotation = Quaternion.Slerp(Controller.WorldRotation, Target.WorldRotation, Time.DeltaTime * RotateSpeed).normalized;
        }
    }
}
