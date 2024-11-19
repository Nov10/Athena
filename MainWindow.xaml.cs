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
        const int width = 1024;
        const int height = 1024;

        DispatcherTimer ImageRefresher = new DispatcherTimer();
        Stopwatch sw = new Stopwatch();
        Camera camera = new Camera
        {
            Position = new Vector3(0, 00, 100f),
            Direction = new Vector3(0, 0f, -1),
            NearPlaneDistance = 3f,
            FarPlaneDistance = 100.0f,
            FieldOfView = 60f,
            AspectRatio = (float)width / height
        };
        PBRRenderer PBR;
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
            PBR = new PBRRenderer(width, height);
            PBR.RenderTarget = new NPhotoshop.Core.Image.NBitmap(width, height);
            PBR.ZBuffer = new float[width, height];
            PBR.ZBuffer2 = new float[width * height];
            MultipleMeshObject TargetMesh = MeshLoader.FBXLoader.LoadFBX_Seperated(@"C:\Mando_Helmet.fbx");
            var t1 = LoadTexture(@"C:\Mando_Helm_Mat_Colour.png");
            var t2 = LoadTexture(@"C:\Helmet_Stand_Mat_Colour.png");
            var t3 = LoadTexture(@"C:\Glass_Mat_Colour.png");
            for (int i = 0; i < TargetMesh.ObjectCount; i++)
            {
                //TargetMesh.Get(i).FragmentShader = new Core.Shader.FragmentShader();
                TargetMesh.Get(i).Shader = new Shader1();
                if(i == 0)
                    ((Shader1)TargetMesh.Get(i).Shader).texture = t2;
                else if (i == 1)
                    ((Shader1)TargetMesh.Get(i).Shader).texture = t3;
                else
                    ((Shader1)TargetMesh.Get(i).Shader).texture = t1;

            }
            PBR.AddObject(TargetMesh);

            ImageRefresher.Interval = TimeSpan.FromTicks(10);
            ImageRefresher.Tick += T_Tick1;
            ImageRefresher.Start();
            RenderTargetImage.Width = width; RenderTargetImage.Height = height;
        }
        long sum = 0;
        int counter = 0;
        private void T_Tick1(object sender, object e)
        {
            //sw.Stop();
            //sw.Restart();

            Render3DScene(0.1f);

            RenderTargetImage.Source = PBR.RenderTarget.ConvertToBitmap();
            //sum += sw.ElapsedTicks;
            //counter++;
            //System.Diagnostics.Debug.WriteLine((float)sum / counter);
        }

        Quaternion q;
        float t = 0;

        private void Render3DScene(float dt)
        {


            t += dt;
            for (int i = 0; i < PBR.Targets.Count; i++)
                //PBR.Targets[i].Rotation = new Vector3(3.14f/2f, 3.14f + t, 0);
                PBR.Targets[i].Rotation = new Vector3(0, t * 0.5f, 0);

            PBR.camera = camera;
            PBR.Render();
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
