using Renderer.Core;
using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.InGame.AirPlane
{
    public class Aircraft : Component
    {
        Core.Object FrontBlade;
        float BladeRotateSpeed;
        float AircraftSpeed;

        public override void Awake()
        {

        }

        public void InitializeAircraft(Core.Object blade, float bladeSpeed, float aircraftSpeed)
        {
            FrontBlade = blade;
            FrontBlade.Parent = Controller;
            BladeRotateSpeed = bladeSpeed;
            AircraftSpeed = aircraftSpeed;
        }

        public override void Start()
        {
            FrontBlade.LocalPosition = new Vector3(0, 0, 2.0f);
        }


        Quaternion TargetRotation;

        Vector3 RotateInputHolder;

        Quaternion CalculateTargetRotation(Vector3 moveInput)
        {
            if (-0.3f >= moveInput.x)
                moveInput.x = -0.3f;
            if (moveInput.x >= 0.3f)
                moveInput.x = 0.3f;
            float inputX = -moveInput.x;
            float inputY = -moveInput.y;
            float inputZ = moveInput.z;
            Vector3 dir = Controller.WorldRotation.RotateVector(new Vector3(inputX, 0, inputZ));
            float angle = MathF.Atan2(dir.x, dir.z) * XMath.Rad2Deg;
            float angleX = MathF.Atan2(inputY, 1) * XMath.Rad2Deg;
            float a = MathF.Atan2(-inputX, 1) * XMath.Rad2Deg_Half;
            return Quaternion.FromEulerAngles(angleX, angle, 0) * Quaternion.FromEulerAngles(0, 0, a);
        }

        public override void Update()
        {
            //Controller.LocalPosition = new Vector3(MathF.Sin(Time.TotalTime * AircraftSpeed), MathF.Sin(Time.TotalTime * AircraftSpeed) * 0.2f, MathF.Cos(Time.TotalTime * AircraftSpeed)) * 5;
            //Vector3 dir = Vector3.Cross(new Vector3(0, 1, 0), Controller.WorldPosition).normalized;           
            //float angle = MathF.Atan2(dir.x, dir.z) * XMath.Rad2Deg;
            //Controller.LocalRotation = Quaternion.FromEulerAngles(0, angle, 0);


            Vector3 moveInput = Input.GetDirectionInput3D(KeyPreset.WASDQE);
            Vector2 moveInput2 = Input.GetDirectionInput2D(KeyPreset.WASD);
            moveInput.x *= 0.5f;
            if (moveInput.sqrMagnitude > 0.5f)
            {
                RotateInputHolder = moveInput;
            }
            if(moveInput.sqrMagnitude > 0.01f)
            {
                moveInput = moveInput.normalized;
            }
            if (moveInput.sqrMagnitude < 0.01f)
            {
                RotateInputHolder.x = moveInput.x;
            }
            else
                RotateInputHolder = moveInput;

            TargetRotation = CalculateTargetRotation(RotateInputHolder);
            Controller.WorldRotation = Quaternion.Slerp(Controller.WorldRotation, TargetRotation, Time.DeltaTime * AircraftSpeed);

            if (moveInput.sqrMagnitude > 0.1f)
            {
                //float inputX = -moveInput.x;
                //float inputY = moveInput.y;
                //float inputZ = moveInput.z;
                //Vector3 dir = Controller.WorldRotation.RotateVector(new Vector3(inputX, 0, inputZ));
                //System.Diagnostics.Debug.WriteLine(dir);
                //float angle = MathF.Atan2(dir.x, dir.z) * XMath.Rad2Deg;
                //float angleX = MathF.Atan2(inputY, 1) * XMath.Rad2Deg;
                //float a = MathF.Atan2(-inputX, 1) * XMath.Rad2Deg_Half;
                //Quaternion q = Quaternion.FromEulerAngles(angleX, angle, 0) * Quaternion.FromEulerAngles(0, 0, a);
                //Controller.WorldRotation = Quaternion.Slerp(Controller.WorldRotation, q, Time.DeltaTime * AircraftSpeed);

                Vector3 zAxis = Controller.WorldRotation.RotateVector(new Vector3(0, 0, 1));
                Vector3 xAxis = (Vector3.Cross(zAxis, new Vector3(0, 1, 0))).normalized;
                Vector3 yAxis = Vector3.Cross(zAxis, xAxis).normalized;

                Controller.WorldPosition = Vector3.Lerp(Controller.WorldPosition, Controller.WorldPosition + (zAxis * moveInput.z + xAxis * moveInput.x).normalized * AircraftSpeed, Time.DeltaTime * AircraftSpeed);
            }
            else
            {

            }
            FrontBlade.LocalRotation = Quaternion.FromEulerAngles(180, 0, Time.TotalTime * BladeRotateSpeed * XMath.Rad2Deg);
        }
    }
}
