using Athena.Engine.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using Microsoft.UI.Xaml;
using System.Timers;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using Athena.Engine.Core.Image;
using System.Reflection;

namespace Athena.Engine.Core
{
    public static class EngineController
    {
        public const int width = 800;
        public const int height = 450;
        public const float AspectRatio = ((float)width / height);
        public static RenderBitmap Window { get; private set; }

        public static List<BaseRenderer> Renderers { get; private set; }
        public static PBRRenderer PBRRenderer { get { return Renderers[0] as PBRRenderer; } }

        public static List<GameObject> WorldObjects;

        public static string DebugText { get; private set; }
        static string _AssetPath;
        public static string AssetPath { get { return _AssetPath; } }

        static DispatcherTimer Updater = new DispatcherTimer();
        static Stopwatch IntervalTimeChecker;
        static Action AfterUpdateEvent;
        public static void Start(Action afterUpdate, UIElement inputHandler)
        {
            _AssetPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Input.Start();
            GPUAccelator.Intialize();
            inputHandler.KeyDown += Input.Content_KeyDown;
            inputHandler.KeyUp += Input.Content_KeyUp;

            Renderers = new List<BaseRenderer>();
            WorldObjects = new List<GameObject>();
            IntervalTimeChecker = new Stopwatch();

            Window = new RenderBitmap(width, height);
            var pbrRenderer = new PBRRenderer(width, height);
            pbrRenderer.LightDirection = new Vector3(-0.5f, -1, 0).normalized * -1;
            Renderers.Add(pbrRenderer);

            AfterUpdateEvent = afterUpdate;
            Updater = new DispatcherTimer();
            Updater.Interval = new TimeSpan(1);
            Updater.Tick += Updater_Tick;
            Updater.Start();
        }

        private static void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            Update();
            AfterUpdateEvent();
        }

        private static void Updater_Tick(object sender, object e)
        {
            Update();
            AfterUpdateEvent();
        }

        public static void StartUpdate()
        {
            Athena.Engine.Core.Time.StartUpdate();
        }

        public static void Update()
        {
            StartUpdate();

            float time_UpdateGameObjects = Update_GameObjects();
            float time_UpdateRenderes = Update_Renderers();

            EndUpdate();

            DebugText =
                $"FPS : {Time.FPS.ToString()}\n" +
                $"Update : {time_UpdateGameObjects}\n" +
                $"Rending : {time_UpdateRenderes}\n" +
                $"-\n" +
                $"Total : {time_UpdateGameObjects + time_UpdateRenderes}";
        }

        public static void EndUpdate()
        {
            Input.Update();
            Athena.Engine.Core.Time.EndUpdate();
        }

        static float Update_GameObjects()
        {
            IntervalTimeChecker.Restart();
            for (int i = 0; i < WorldObjects.Count; i++)
            {
                if (WorldObjects[i].IsWorldActive == true)
                    WorldObjects[i].UpdateGameObject();
            }
            return (float)IntervalTimeChecker.Elapsed.TotalSeconds;
        }

        static float Update_Renderers()
        {
            IntervalTimeChecker.Restart();
            for (int i = 0; i < Renderers.Count; i++)
            {
                Renderers[i].Render(Athena.Engine.Core.MeshRenderer.RendererList);
            }
            return (float)IntervalTimeChecker.Elapsed.TotalSeconds;
        }
    }
}
