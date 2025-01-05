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

        public override void Update()
        {
            //Controller.LocalPosition = new Vector3(MathF.Sin(Time.TotalTime * AircraftSpeed), MathF.Sin(Time.TotalTime * AircraftSpeed) * 0.2f, MathF.Cos(Time.TotalTime * AircraftSpeed)) * 5;
            //Vector3 dir = Vector3.Cross(new Vector3(0, 1, 0), Controller.WorldPosition).normalized;           
            //float angle = MathF.Atan2(dir.x, dir.z) * XMath.Rad2Deg;
            //Controller.LocalRotation = Quaternion.FromEulerAngles(0, angle, 0);


            var moveInput = Input.GetDirectionInput(KeyPreset.WASD);

            if(moveInput.sqrMagnitude() > 0.5f)
            {
                Vector3 dir = Controller.WorldRotation.RotateVector(new Vector3(-moveInput.x, 0, moveInput.y));
                float angle = MathF.Atan2(dir.x, dir.z) * XMath.Rad2Deg;
                Controller.WorldRotation = Quaternion.Slerp(Controller.WorldRotation, Quaternion.FromEulerAngles(0, angle, 0), Time.ElapsedDeltaTime * AircraftSpeed);
                
                Vector3 zAxis = Controller.WorldRotation.RotateVector(new Vector3(0, 0, 1));
                Vector3 xAxis = (Vector3.Cross(zAxis, new Vector3(0, 1, 0))).normalized;
                Vector3 yAxis = Vector3.Cross(zAxis, xAxis).normalized;

                Controller.WorldPosition = Vector3.Lerp(Controller.WorldPosition, Controller.WorldPosition + (zAxis * moveInput.y + xAxis * moveInput.x).normalized, Time.ElapsedDeltaTime * AircraftSpeed);
            }
            FrontBlade.LocalRotation = Quaternion.FromEulerAngles(180, 0, Time.TotalTime * BladeRotateSpeed * XMath.Rad2Deg);
        }
    }
}
