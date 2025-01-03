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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Renderer
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        const int width = 768;
        const int height = 768;

        DispatcherTimer ImageRefresher = new DispatcherTimer();
        Stopwatch sw = new Stopwatch();
        Camera camera = new Camera
        {
            Position = new Vector3(0, 00, 100f),
            Direction = new Vector3(0, 0f, -1),
            NearPlaneDistance = 3f,
            FarPlaneDistance = 500.0f,
            FieldOfView = 60f,
            AspectRatio = (float)width / height
        };
        public static PBRRenderer MainRenderer;
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

        public MainWindow()
        {
            InitializeComponent();

            WorldObjects = new List<Core.Object>();
            //Core.Object airplane = new Core.Object();
            MainRenderer = new PBRRenderer(width, height);
            MainRenderer.RenderTarget = new NPhotoshop.Core.Image.NBitmap(width, height);
            MainRenderer.ZBuffer = new float[width * height];
            MainRenderer.LightDirection = new Vector3(0, -1, 0).normalized * -1;
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


            renderer = MeshLoader.FBXLoader.LoadFBX_SeperatedAsRenderer(@"C:\p.stl");
            Core.Object plane = new Core.Object();
            Core.Renderer planeRenderer = new Core.Renderer();
            planeRenderer.RenderDatas.Add(renderer.RenderDatas[0]);
            planeRenderer.RenderDatas[0].Shader = new SimpleColorShader(new Color(255, 255, 255, 255));
            plane.LocalPosition = new Vector3(0, -5, 0);
            plane.AddComponent(planeRenderer);
            //var normalTex = LoadTexture(@"C:\Normal.png");
            //renderer.RenderDatas = new List<RenderData>(new RenderData[] { renderer.RenderDatas[0] });
            //System.Diagnostics.Debug.WriteLine(renderer.RenderDatas.Count);
            //for(int i =0; i < renderer.RenderDatas.Count; i++)
            //{
            //    renderer.RenderDatas[i].Shader = new SimpleColorShader(new Color(255, 255, 255, 255));
            //}
            //renderer.RenderDatas[0].Shader = new Shader1();
            //(renderer.RenderDatas[0].Shader as Shader1).MainTexture = bodyTex;
            //(renderer.RenderDatas[0].Shader as Shader1).NormalTexture = normalTex;
            //airplane.AddComponent(renderer);
            //airplane.Rotation = new Vector3(-90f * 3.141592f / 180f, 0, 0); 
            blade.Parent = body;
            WorldObjects.Add(body);
            WorldObjects.Add(blade);
            WorldObjects.Add(plane);


            // MultipleMeshObject TargetMesh = MeshLoader.FBXLoader.LoadFBX_Seperated(@"C:\Mando_Helmet.fbx");
            //var t1 = LoadTexture(@"C:\Mando_Helm_Mat_Colour.png");
            //var t2 = LoadTexture(@"C:\Helmet_Stand_Mat_Colour.png");
            //var t3 = LoadTexture(@"C:\Glass_Mat_Colour.png");
            //var normal_texture1 = LoadTexture(@"C:\Mando_Helm_Mat_Normal.png");
            //var normal_texture2 = LoadTexture(@"C:\Helmet_Stand_Mat_Normal.png");
            //var normal_texture3 = LoadTexture(@"C:\Glass_Mat_BakeNormal.png");
            //for (int i = 0; i < TargetMesh.ObjectCount; i++)
            //{
            //    //TargetMesh.Get(i).FragmentShader = new Core.Shader.FragmentShader();
            //    TargetMesh.Get(i).Shader = new Shader1();
            //    if(i == 0)
            //    {
            //        ((Shader1)TargetMesh.Get(i).Shader).MainTexture = t2;
            //        ((Shader1)TargetMesh.Get(i).Shader).NormalTexture = normal_texture2;
            //    }
            //    else if (i == 1)
            //    {
            //        ((Shader1)TargetMesh.Get(i).Shader).MainTexture = t3;
            //        ((Shader1)TargetMesh.Get(i).Shader).NormalTexture = normal_texture3;
            //    }
            //    else
            //    {
            //        ((Shader1)TargetMesh.Get(i).Shader).MainTexture = t1;
            //        ((Shader1)TargetMesh.Get(i).Shader).NormalTexture = normal_texture1;
            //    }

            //}
            //PBR.AddObject(renderer);

            ImageRefresher.Interval = TimeSpan.FromTicks(5);
            ImageRefresher.Tick += T_Tick1;
            ImageRefresher.Start();
            
            RenderTargetImage.Width = 1024; RenderTargetImage.Height = 1024;
        }
        long sum = 0;
        int counter = 0;
        List<Core.Object> WorldObjects;
        float Time;
        private void T_Tick1(object sender, object e)
        {
            //sw.Stop();
            //sw.Restart();
            Time += 0.01f;

            MainRenderer.Targets.Clear();
            WorldObjects[1].LocalRotation = Quaternion.FromEulerAngles(180, 0, Time * 8* 10 * XMath.Rad2Deg);
            
            WorldObjects[1].LocalPosition = new Vector3(0, 0, 2.0f);
            WorldObjects[2].LocalRotation = Quaternion.FromEulerAngles(90, 0, 0);

            WorldObjects[0].LocalPosition = new Vector3(MathF.Sin(Time * 10), MathF.Sin(Time * 8) * 0.2f, MathF.Cos(Time * 10)) * 10;
            Vector3 dir = Vector3.Cross(new Vector3(0, 1, 0), WorldObjects[0].WorldPosition);
            dir = dir.normalized;

            // atan2( X성분, Z성분 ) = 현재 기준(앞=+Z)에서 dir 벡터가 얼마나 회전되어있는가
            float angle = MathF.Atan2(dir.x, dir.z) * 180 / 3.141592f;

            // 오브젝트 회전 설정
            WorldObjects[0].LocalRotation = Quaternion.FromEulerAngles(0, angle, 0);
            //(WorldObjects[0].Components[0] as Core.Renderer).RenderDatas[1].Rotation = new Vector3(-90 * 3.141592f / 180f * 0, 0, Time);
            for (int i = 0; i<WorldObjects.Count; i++)
            {
                WorldObjects[i].Update();
            }
            MainRenderer.camera = camera;
            MainRenderer.Render();

            RenderTargetImage.Source = MainRenderer.RenderTarget.ConvertToBitmap();
        }

        Quaternion q;
        float t = 0;

        private void Render3DScene(float dt)
        {


            t += dt;
            //for (int i = 0; i < PBR.Targets.Count; i++)
            //    //PBR.Targets[i].Rotation = new Vector3(3.14f/2f, 3.14f + t, 0);
            //    PBR.Targets[i].Rotation = new Vector3(-90 * 3.141592f/180f, t * 0.5f, 0);

            MainRenderer.camera = camera;
            MainRenderer.Render();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            //myButton.Content = "Clicked";
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var k = e.Key;
            Vector3 v = new Vector3(0, 0, 0);
            if (k == Windows.System.VirtualKey.W)
                v = new Vector3(0, 0, -1);
            else if (k == Windows.System.VirtualKey.S)
                v = new Vector3(0, 0, 1);
            if (k == Windows.System.VirtualKey.A)
                v = new Vector3(1, 0, 0);
            else if (k == Windows.System.VirtualKey.D)
                v = new Vector3(-1, 0, 0);
            if (k == Windows.System.VirtualKey.Q)
                v = new Vector3(0, 1, 0);
            else if (k == Windows.System.VirtualKey.E)
                v = new Vector3(0, -1, 0);

            if (k == Windows.System.VirtualKey.Up)
                q = Quaternion.CreateRotationQuaternion(new Vector3(1, 0, 0), -5);
            else if (k == Windows.System.VirtualKey.Down)
                q = Quaternion.CreateRotationQuaternion(new Vector3(1, 0, 0), 5);
            else if (k == Windows.System.VirtualKey.Left)
                q = Quaternion.CreateRotationQuaternion(new Vector3(0, 1, 0), 5);
            else if (k == Windows.System.VirtualKey.Right)
                q = Quaternion.CreateRotationQuaternion(new Vector3(0, 1, 0), -5);
            else
                q = new Quaternion(1, 0, 0, 0);
            camera.Direction = q.RotateVector(camera.Direction);

            Vector3 zAxis = camera.Direction.normalized;
            Vector3 xAxis = (Vector3.Cross(camera.WorldUp, zAxis)).normalized;
            Vector3 yAxis = Vector3.Cross(zAxis, xAxis).normalized;

            Matrix4x4 rotationMatrix = new Matrix4x4(
                xAxis.x, yAxis.x, -zAxis.x, 0,
                xAxis.y, yAxis.y, -zAxis.y, 0,
                xAxis.z, yAxis.z, -zAxis.z, 0,
                0, 0, 0, 1);

            camera.Position += (-zAxis * v.z + xAxis * v.x + new Vector3(0, v.y, 0)) * 5;
            //camera.Position += (v) * 1;
        }
    }
}
