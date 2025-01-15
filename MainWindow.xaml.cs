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
using Athena.InGame.Terrain;
using Athena.Engine.Core;
using Athena.Engine.Core.Image;
using Athena.Engine.Core.Rendering;
using Athena.InGame.Ring;
using Athena.Engine.Core.Rendering.Shaders;
using Windows.ApplicationModel;
using Windows.Storage;
using Microsoft.UI.Xaml.Shapes;
using Windows.Management.Core;
using System.Reflection;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Athena
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private NBitmap LoadTexture(string path)
        {
            path = EngineController.AssetPath + path;
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
                texture.ConvertFromByteArray(pixelData, textureBitmap.PixelWidth, textureBitmap.PixelHeight);
                return texture;
            }
        }
        RenderBitmap SubWindow;
        public MainWindow()
        {
            InitializeComponent();

            EngineController.Start(Update, this.Content);

            GameObject flowManager = new GameObject();
            flowManager.AddComponent(new GameFlowManager());
            var renderer = MeshLoader.FBXLoader.LoadFBX_SeperatedAsRenderer("Aereo.fbx");
            var bodyTex = LoadTexture("body.png");
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

            var ringGenerator = new RingLineGenerator();
            GameObject rg = new GameObject();
            rg.AddComponent(ringGenerator);

            Camera cameraComponent = new Camera
            {
                NearPlaneDistance = 1f,
                FarPlaneDistance = 300.0f,
                FieldOfView = 60f,
                AspectRatio = EngineController.AspectRatio
            };
            GameObject camera = new GameObject();
            camera.WorldPosition = new Vector3(0, 0, 60);
            camera.WorldRotation = Quaternion.FromEulerAngles(0, 180, 0);
            //cameraComponent.MainRenderer = mainRenderer;
            cameraComponent.SetRenderTarget(EngineController.Window);
            camera.AddComponent(cameraComponent);
            CameraController camControl = new CameraController();
            camControl.Target = body;
            camera.AddComponent(camControl);
            camControl.RotateSpeed = 2;
            camControl.MoveSpeed = 3;
            BaseRenderer.RegisterCameraToRenderer(cameraComponent, EngineController.PBRRenderer);

            blade.Parent = body;
            //plane.LocalRotation = Quaternion.FromEulerAngles(180, 0, 0);

            Aircraft aircraft = new Aircraft();
            body.AddComponent(aircraft);
            body.WorldPosition = new Vector3(0, 20, -20);
            aircraft.InitializeAircraft(blade, 3, 5);

            GameFlowManager.Instance.Player = aircraft;
            camera.WorldPosition += new Vector3(0, 50, 0);


            GameObject map = new GameObject();

            MapGenerator generator =
                new MapGenerator(noiseScale: 25, octave: 7, persistance: 0.518f, lacunarity: 2.5f, heightMultiplier: 3,
                regions: new TerrainType[] {
                new TerrainType("Water Deep", 0.0f, new Color(0, 90, 255, 255)),
                new TerrainType("Water Deep", 0.69f, new Color(0, 120, 255, 255)),
                new TerrainType("Grass1", 0.65f, new Color(56, 255, 0, 255)),
                new TerrainType("Grass2", 0.85f, new Color(41, 152, 68, 255)),
                new TerrainType("Snow", 1.25f, new Color(255, 255, 255, 255))
                });
            generator.NormalizeMode = Noise.eNormalizeMode.Global;
            generator.Seed = 0;
            generator.offset = new Vector2(0, 0);

            EndlessTerrain terrain = new EndlessTerrain();
            terrain.Initialize(generator);
            terrain.Viewer = body;
            terrain.cam = cameraComponent;
            EndlessTerrain.LODInfo info0 = new EndlessTerrain.LODInfo(2, 50);
            EndlessTerrain.LODInfo info1 = new EndlessTerrain.LODInfo(4, 100);
            EndlessTerrain.LODInfo info2 = new EndlessTerrain.LODInfo(8, 350);
            terrain.DetailLevels = new EndlessTerrain.LODInfo[] { info0, info1 };

            map.AddComponent(generator);
            map.AddComponent(terrain);


            RenderTargetImage.Width = 1920; RenderTargetImage.Height = 1080;
        }

        //float Time;
        private void Update()
        {
            //Debugger.Text = "FUCK";
            //Debugger.Text = Assembly.GetExecutingAssembly().Location;// ApplicationDataManager.CreateForPackageFamily(Package.Current.Id.FamilyName).LocalFolder.Path;
            RenderTargetImage.Source = EngineController.Window.ConvertToBitmap();
            Debugger.Text = EngineController.DebugText;
        }
    }
}
