using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Renderer.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Renderer.Maths;
using Renderer.Renderer.PBR;
using System.Timers;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using System.Diagnostics;
using Renderer.Core.PBR;
using Renderer;
using Renderer.Core;
using Assimp.Unmanaged;
using NPhotoshop.Core.Image;
using Windows.Graphics.Imaging;
using Renderer.InGame.AirPlane;
using Renderer.InGame;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Renderer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        const int width = 960;
        const int height = 540;

        DispatcherTimer ImageRefresher = new DispatcherTimer();
        Stopwatch sw = new Stopwatch();

        Timer tTimer;
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
        NBitmap Window;
        public MainWindow()
        {
            InitializeComponent();

            this.Content.KeyDown += Input.Content_KeyDown;
            this.Content.KeyUp += Input.Content_KeyUp;
            Input.Start();
            WorldObjects = new List<Core.Object>();

            var renderer = MeshLoader.FBXLoader.LoadFBX_SeperatedAsRenderer(@"C:\Aereo.fbx");
            var bodyTex = LoadTexture(@"C:\body.png");
            Core.Object body = new Core.Object();
            Core.Renderer bodyRenderer = new Core.Renderer();
            bodyRenderer.RenderDatas.Add(renderer.RenderDatas[0]);
            bodyRenderer.RenderDatas[0].Shader = new Shader1();
            (bodyRenderer.RenderDatas[0].Shader as Shader1).MainTexture = bodyTex;
            body.AddComponent(bodyRenderer);

            Core.Object blade = new Core.Object();
            Core.Renderer bladeRenderer = new Core.Renderer();
            bladeRenderer.RenderDatas.Add(renderer.RenderDatas[1]);
            bladeRenderer.RenderDatas[0].Shader = new SimpleColorShader(new Color(255, 255, 255, 255));
            blade.LocalPosition = new Vector3(0, 0, 2.0f);
            blade.AddComponent(bladeRenderer);


            renderer = MeshLoader.FBXLoader.LoadFBX_SeperatedAsRenderer(@"C:\untitled.fbx");
            var planeTex = LoadTexture(@"C:\Untitled.png");
            Core.Object plane = new Core.Object();
            Core.Renderer planeRenderer = new Core.Renderer();
            planeRenderer.RenderDatas.Add(renderer.RenderDatas[0]);
            //planeRenderer.RenderDatas[0].Shader = new SimpleColorShader(new Color(255, 255, 255, 255));
            planeRenderer.RenderDatas[0].Shader = new Shader1();
            (planeRenderer.RenderDatas[0].Shader as Shader1).MainTexture = bodyTex;
            plane.LocalScale = new Vector3(45, 2, 45);
            plane.LocalPosition = new Vector3(0, -5, 0);
            plane.AddComponent(planeRenderer);

            Camera cameraComponent = new Camera
            {
                NearPlaneDistance = 1f,
                FarPlaneDistance = 500.0f,
                FieldOfView = 60f,
                AspectRatio = (float)width / height
            };
            Core.Object camera = new Core.Object();
            camera.WorldPosition = new Vector3(0, 0, 60);
            camera.WorldRotation = Quaternion.FromEulerAngles(0, 180, 0);
            cameraComponent.MainRenderer = new PBRRenderer(width, height);
            Window = new NBitmap(width, height);
            cameraComponent.MainRenderer.RenderTarget = Window;
            cameraComponent.MainRenderer.LightDirection = new Vector3(-0.5f, -1, 0).normalized * -1;
            camera.AddComponent(cameraComponent);
            CameraController camControl = new CameraController();
            camControl.Target = body;
            camera.AddComponent(camControl);
            camControl.RotateSpeed = 2;
            camControl.MoveSpeed = 3;

            //renderer = MeshLoader.FBXLoader.LoadFBX_SeperatedAsRenderer(@"C:\cam.stl");
            //Core.Object cam = new Core.Object();
            //Core.Renderer camRenderer = new Core.Renderer();
            //camRenderer.RenderDatas.Add(renderer.RenderDatas[0]);
            //camRenderer.RenderDatas[0].Shader = new SimpleColorShader(new Color(255, 255, 255, 255));
            //cam.LocalPosition = new Vector3(0, 0, 2.0f);
            //cam.AddComponent(camRenderer);

            blade.Parent = body;
            plane.LocalRotation = Quaternion.FromEulerAngles(180, 0, 0);

            Aircraft aircraft = new Aircraft();            
            body.AddComponent(aircraft);
            aircraft.InitializeAircraft(blade, 3, 10);

            WorldObjects.Add(body);
            WorldObjects.Add(blade);
            WorldObjects.Add(camera);
            //WorldObjects.Add(cam);
            WorldObjects.Add(plane);

            camera.WorldPosition += new Vector3(0, 20, 0);

            ImageRefresher.Interval = TimeSpan.FromTicks(10);
            ImageRefresher.Tick += T_Tick1;
            ImageRefresher.Start();
            
            RenderTargetImage.Width = 1920; RenderTargetImage.Height = 1080;
        }
        long sum = 0;
        int counter = 0;
        public static List<Core.Object> WorldObjects;

        //float Time;
        private void T_Tick1(object sender, object e)
        {
            Core.Time.StartUpdate();

            for (int i = 0; i<WorldObjects.Count; i++)
            {
                WorldObjects[i].Update();
            }
            for (int i = 0; i < WorldObjects.Count; i++)
            {
                if(WorldObjects[i].TryGetComponent(out Camera cam))
                {
                    cam.Render(WorldObjects);
                }
            }

            RenderTargetImage.Source = Window.ConvertToBitmap();
            Input.Update();
            Core.Time.EndUpdate();
        }
    }
}
