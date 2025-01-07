using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Core
{
    public static class Time
    {
        static Stopwatch Timer = new Stopwatch();
        static long Tick;
        static long DeltaTick;
        public static float DeltaTime
        {
            get { return (float)TimeSpan.FromTicks(DeltaTick).TotalSeconds; }
        }
        public static float TotalTime
        {
            get { return (float)TimeSpan.FromTicks(Tick).TotalSeconds; }
        }
        public static int TotalTimeInt
        {
            get { return (int)TimeSpan.FromTicks(Tick).TotalSeconds; }
        }

        public static int FPS
        {
            get { return FPSCounter; }
        }
        static int preTimeInt;
        static int FPSCounter = 0;
        public static void StartUpdate()
        {
            preTimeInt = Core.Time.TotalTimeInt;

            DeltaTick = Timer.ElapsedTicks;
            Tick += DeltaTick;
            Timer.Restart();

            if (preTimeInt < Core.Time.TotalTimeInt)
            {
                System.Diagnostics.Debug.WriteLine($"FPS : {FPS}");
                FPSCounter = 0;
            }
            FPSCounter++;
        }

        public static void EndUpdate()
        {

        }
    }
}
