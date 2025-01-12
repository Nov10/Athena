using Athena.Maths;
using Athena.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.InGame.AirPlane
{
    public class Aircraft : Component
    {
        GameObject FrontBlade;
        float BladeRotateSpeed;
        float AircraftSpeed;

        public override void Awake()
        {

        }

        public void InitializeAircraft(GameObject blade, float bladeSpeed, float aircraftSpeed)
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
        Vector3 NowMoveInput;

        Quaternion CalculateTargetRotation(Vector3 moveInput)
        {
            float inputX = -moveInput.x;
            float inputY = -moveInput.y;
            float inputZ = moveInput.z;
            Vector3 dir = Controller.WorldRotation.RotateVector(new Vector3(inputX, 0, inputZ));
            float angleY = MathF.Atan2(dir.x, dir.z) * XMath.Rad2Deg;
            float angleX = MathF.Atan2(inputY, 1) * XMath.Rad2Deg;
            float angleZ = MathF.Atan2(-inputX, 1) * XMath.Rad2Deg_Half;
            return Quaternion.FromEulerAngles(angleX, angleY, 0) * Quaternion.FromEulerAngles(0, 0, angleZ);
        }
        Vector3 CalculateTargetDirection(Vector3 moveInput)
        {
            //+ Controller.Right * moveInput.x
            return (Controller.Forward * moveInput.z ).normalized;
        }

        public override void Update()
        {
            Vector3 moveInput = Input.GetDirectionInput3DXZY(KeyPreset.WASDQE);
            moveInput.z = 1;
            moveInput.x *= 0.5f;

            //입력
            if(moveInput.sqrMagnitude > 0.01f)
            {
                moveInput = moveInput.normalized;
            }

            NowMoveInput = Vector3.Lerp(NowMoveInput, moveInput, 5 * Time.DeltaTime);

            if (-0.15f >= NowMoveInput.x)
                NowMoveInput.x = -0.15f;
            if (NowMoveInput.x >= 0.15f)
                NowMoveInput.x = 0.15f;

            TargetRotation = CalculateTargetRotation(NowMoveInput);
            Controller.WorldRotation = Quaternion.Slerp(Controller.WorldRotation, TargetRotation, Time.DeltaTime * AircraftSpeed);

            Vector3 dir = CalculateTargetDirection(NowMoveInput);
            Controller.WorldPosition += dir * AircraftSpeed * Time.DeltaTime;

            FrontBlade.LocalRotation = Quaternion.FromEulerAngles(180, 0, Time.TotalTime * BladeRotateSpeed * XMath.Rad2Deg);
        }
    }
}
