using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Engine.Core
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

        public static int FPS { get; private set; }
        public static bool IsTimeIntChanged;
        static int preTimeInt;
        static int FPSCounter = 0;
        public static void StartUpdate()
        {
            IsTimeIntChanged = false;
            DeltaTick = Timer.ElapsedTicks;
            Tick += DeltaTick;
            Timer.Restart();

            if (preTimeInt != TotalTimeInt)
            {
                FPS = FPSCounter;
                preTimeInt = TotalTimeInt;
                IsTimeIntChanged = true;
                FPSCounter = 0;
            }
            FPSCounter++;
        }


        public static void EndUpdate()
        {

        }
    }
}
