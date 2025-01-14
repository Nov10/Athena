using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Athena.Maths;
using System.Timers;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using System.Diagnostics;
using Athena;
using Assimp.Unmanaged;
using Windows.Graphics.Imaging;
using Athena.InGame.AirPlane;
using Athena.InGame;
using Athena.Terrain;
using Athena.Engine.Core;
using Athena.Engine.Core.Image;
using Athena.Engine.Core.Rendering;
using Athena.InGame.Ring;
using Renderer.InGame.Ring;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Athena
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        const int width = 800;
        const int height = 450;

        DispatcherTimer ImageRefresher = new DispatcherTimer();


        private NBitmap LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Texture file not found: {path}");
            }

            // 이미지 파일 로드
            using (var stream = File.OpenRead(path))
            {
                var decoder = BitmapDecoder.CreateAsync(stream.AsRandomAccessStream()).AsTask().Result;
                var textureBitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);

                var pixelBuffer = decoder.GetPixelDataAsync().AsTask().Result;
                var pixelData = pixelBuffer.DetachPixelData();
                NBitmap texture = new NBitmap(textureBitmap.PixelWidth, textureBitmap.PixelHeight);
                texture.ConvertFromBitmap(pixelData, textureBitmap.PixelWidth, textureBitmap.PixelHeight);
                return texture;
            }
        }
        RenderBitmap Window;
        RenderBitmap SubWindow;
        public MainWindow()
        {
            InitializeComponent();

            this.Content.KeyDown += Input.Content_KeyDown;
            this.Content.KeyUp += Input.Content_KeyUp;
            Input.Start();
            GPUAccelator.Intialize();
            var mainRenderer = new PBRRenderer(width, height);
            WorldObjects = new List<GameObject>();
            GameObject flowManager = new GameObject();
            var manager = new GameFlowManager();
            flowManager.AddComponent(manager);

            var renderer = MeshLoader.FBXLoader.LoadFBX_SeperatedAsRenderer(@"C:\Aereo.fbx");
            var bodyTex = LoadTexture(@"C:\body.png");
            GameObject body = new GameObject();
            
            Athena.Engine.Core.MeshRenderer bodyRenderer = new Athena.Engine.Core.MeshRenderer();
            bodyRenderer.RenderDatas.Add(renderer.RenderDatas[0]);
            bodyRenderer.RenderDatas[0].Shader = new NormalShader();
            (bodyRenderer.RenderDatas[0].Shader as NormalShader).MainTexture = bodyTex;
            body.AddComponent(bodyRenderer);

            GameObject blade = new GameObject();
            Athena.Engine.Core.MeshRenderer bladeRenderer = new Athena.Engine.Core.MeshRenderer();
            bladeRenderer.RenderDatas.Add(renderer.RenderDatas[1]);
            bladeRenderer.RenderDatas[0].Shader = new Athena.Engine.Core.Rendering.Shaders.SimpleColorShader(new Color(255, 255, 255, 255));
            blade.LocalPosition = new Vector3(0, 0, 2.0f);
            blade.AddComponent(bladeRenderer);

            //var ring1 = CircleRing.CreateRingObject(6.0f);
            //ring1.Controller.WorldPosition = new Vector3(0, 50, 16);
            //var ring2 = CircleRing.CreateRingObject(7.5f);
            //ring2.Controller.WorldPosition = new Vector3(0, 60, 50);
            //ring2.Controller.WorldRotation = Quaternion.FromEulerAngles(-40, 180, 0);
            ////var ring3 = CircleRing.CreateRingObject(1.5f);
            ////ring3.Controller.WorldPosition = new Vector3(0, 50, 32);

            //ring1.Controller.WorldRotation = Quaternion.FromEulerAngles(0, 180, 0);
            // ring2.Controller.WorldRotation = Quaternion.FromEulerAngles(0, 180, 0);
            //ring3.Controller.WorldRotation = Quaternion.FromEulerAngles(0, 180, 0);

            //GameObject cube = new GameObject();
            //var cubeData = RenderData.CreateCube1x1();
            //renderer = new MeshRenderer();
            //cubeData.Shader = new SimpleColorShader(new Color(255, 255, 255, 255));
            //renderer.RenderDatas.Add(cubeData);
            //cube.AddComponent(renderer);
            //cube.WorldPosition = new Vector3(0, 50, 16);

            var ringGenerator = new RingLineGenerator();
            GameObject rg = new GameObject();
            rg.AddComponent(ringGenerator);



            renderer = MeshLoader.FBXLoader.LoadFBX_SeperatedAsRenderer(@"C:\untitled.fbx");
            var planeTex = LoadTexture(@"C:\Untitled.png");
            GameObject plane = new GameObject();
            Athena.Engine.Core.MeshRenderer planeRenderer = new Athena.Engine.Core.MeshRenderer();
            planeRenderer.RenderDatas.Add(renderer.RenderDatas[0]);
            planeRenderer.RenderDatas[0].Shader = new NormalShader();
            (planeRenderer.RenderDatas[0].Shader as NormalShader).MainTexture = bodyTex;
            plane.LocalScale = new Vector3(45, 2, 45);
            plane.LocalPosition = new Vector3(0, -5, 0);
            plane.AddComponent(planeRenderer);



            Camera cameraComponent = new Camera
            {
                NearPlaneDistance = 1f,
                FarPlaneDistance = 100.0f,
                FieldOfView = 60f,
                AspectRatio = (float)width / height
            };
            GameObject camera = new GameObject();
            camera.WorldPosition = new Vector3(0, 0, 60);
            camera.WorldRotation = Quaternion.FromEulerAngles(0, 180, 0);
            Window = new RenderBitmap(width, height);
            mainRenderer.LightDirection = new Vector3(-0.5f, -1, 0).normalized * -1;
            cameraComponent.MainRenderer = mainRenderer;
            cameraComponent.SetRenderTarget(Window);
            camera.AddComponent(cameraComponent);
            CameraController camControl = new CameraController();
            camControl.Target = body;
            camera.AddComponent(camControl);
            camControl.RotateSpeed = 2;
            camControl.MoveSpeed = 3;

            //Camera cameraComponent2 = new Camera
            //{
            //    NearPlaneDistance = 1f,
            //    FarPlaneDistance = 80.0f,
            //    FieldOfView = 60f,
            //    AspectRatio = (float)width / height
            //};
            //GameObject camera2 = new GameObject();
            //var subRenderer = new PBRRenderer((int)(width * 0.2f), (int)(height * 0.2f));
            //camera2.Parent = body;
            //camera2.LocalPosition = new Vector3(10, 0, 0);
            //camera2.LocalRotation = Quaternion.FromEulerAngles(0, -90, 0);
            //SubWindow = new RenderBitmap((int)(width * 0.2f),(int)( height * 0.2f));
            //mainRenderer.LightDirection = new Vector3(-0.5f, -1, 0).normalized * -1;
            //RenderTargetImage2.Width = (int)(width * 0.45f);
            //RenderTargetImage2.Height = (int)(height * 0.45f);
            //cameraComponent2.MainRenderer = subRenderer;
            //cameraComponent2.SetRenderTarget(SubWindow);
            //camera2.AddComponent(cameraComponent2);

            blade.Parent = body;
            plane.LocalRotation = Quaternion.FromEulerAngles(180, 0, 0);

            Aircraft aircraft = new Aircraft();            
            body.AddComponent(aircraft);
            body.WorldPosition = new Vector3(0, 20, 0);
            aircraft.InitializeAircraft(blade, 3, 5);

            manager.Player = aircraft;
            camera.WorldPosition += new Vector3(0, 50, 0);


            GameObject map = new GameObject();
            Athena.Engine.Core.MeshRenderer mapRenderer = new Athena.Engine.Core.MeshRenderer();
            mapRenderer.RenderDatas = new List<RenderData>();
            mapRenderer.RenderDatas.Add(new RenderData());
            MapDisplay display = new MapDisplay();
            display.RenderComponenet = mapRenderer;

            MapGenerator generator = new MapGenerator();
            generator.EditorPreviewLevlOfDetail = 0;
            generator.DrawMode = MapGenerator.eDrawaMode.DrawMesh;
            generator.NormalizeMode = Noise.eNormalizeMode.Global;
            generator.NoiseScale = 25;
            generator.Octaves = 5;
            generator.Persistance = 0.418f;
            generator.Lacunarity = 5;
            generator.MeshHeightMultiplier = 5;
            generator.Seed = 0;
            generator.offset = new Vector2(0, 0);
            //generator.AutoUpdate = true;

            List<TerrainType> types = new List<TerrainType>();
            TerrainType waterDeep = new TerrainType("Water Deep", 0.1f, new Color(0, 90, 255, 255));
            TerrainType grass1 = new TerrainType("Grass1", 0.30f, new Color(56, 255, 0, 255));
            TerrainType grass2 = new TerrainType("Grass2", 0.65f, new Color(41, 152, 68, 255));
            TerrainType snow = new TerrainType("Snow", 0.95f, new Color(255, 255, 255, 255));
            types.Add(waterDeep);
            types.Add(grass1);
            types.Add(grass2);
            types.Add(snow);

            generator.Regions = types.ToArray();

            EndlessTerrain terrain = new EndlessTerrain();
            terrain.Initialize(generator);
            terrain.Viewer = body;
            terrain.cam = cameraComponent;
            t = terrain;
            EndlessTerrain.LODInfo info0 = new EndlessTerrain.LODInfo(4, 50);
            EndlessTerrain.LODInfo info1 = new EndlessTerrain.LODInfo(8, 100); 
            EndlessTerrain.LODInfo info2 = new EndlessTerrain.LODInfo(12, 150);
            terrain.DetailLevels = new EndlessTerrain.LODInfo[] { info0, info1 };

            map.AddComponent(mapRenderer);
            map.AddComponent(display);
            map.AddComponent(generator);
            map.AddComponent(terrain);


            //generator.DrawMapInEditor();
            ImageRefresher.Interval = TimeSpan.FromTicks(1);
            ImageRefresher.Tick += T_Tick1;
            ImageRefresher.Start();
            
            RenderTargetImage.Width = 1920; RenderTargetImage.Height = 1080;
        }
        EndlessTerrain t;
        long sum = 0;
        int counter = 0;
        public static List<GameObject> WorldObjects;

        Stopwatch IntervalTimeChecker = new Stopwatch();
        Stopwatch IntervalTotalTimeChecker = new Stopwatch();
        string fpsLine;
        //float Time;
        private void T_Tick1(object sender, object e)
        {
            string debugger = string.Empty;
            if (Time.IsTimeIntChanged)
                debugger += (fpsLine = $"FPS : {Time.FPS.ToString()}");
            else
                debugger += fpsLine;

            Athena.Engine.Core.Time.StartUpdate();
            IntervalTotalTimeChecker.Restart();
            IntervalTimeChecker.Restart();
            for (int i = 0; i<WorldObjects.Count; i++)
            {
                if(WorldObjects[i].IsWorldActive == true)
                    WorldObjects[i].Update();
            }
            double t_update = IntervalTimeChecker.Elapsed.TotalSeconds;
            IntervalTimeChecker.Restart();
            for (int i = 0; i < Camera.CameraList.Count; i++)
            {
                Camera.CameraList[i].Render(Athena.Engine.Core.MeshRenderer.RendererList);
            }
            RenderTargetImage.Source = Window.ConvertToBitmap();
            double t_Rendring = IntervalTimeChecker.Elapsed.TotalSeconds;

            debugger += $"\nUpdate : {t_update}\nRending : {t_Rendring}\n-\nTotal : {IntervalTotalTimeChecker.Elapsed.TotalSeconds}";
            FPSDebugger.Text = debugger;
            //RenderTargetImage2.Source = SubWindow.ConvertToBitmap();
            //Debugger.Text = t.Get();

            Input.Update();
            Athena.Engine.Core.Time.EndUpdate();
        }
    }
}
