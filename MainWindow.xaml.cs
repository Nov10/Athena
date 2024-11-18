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
        //ImageSourceConverter converter = new ImageSourceConverter();
        public MainWindow()
        {
            InitializeComponent();
            PBR = new PBRRenderer();
            PBR.RenderTarget = new NPhotoshop.Core.Image.NBitmap(width, height);
            PBR.ZBuffer = new float[width, height];
            PBR.ZBuffer2 = new float[width * height];
            PBR.width = width;
            PBR.height = height;
            //ose ember.obj
            for (int i = 0; i < 1; i++)
            {
                WireFrameObject TargetMesh = MeshLoader.FBXLoader.LoadFBX_WireFrameObject(@"C:\Mando_Helmet.fbx");
                TargetMesh.Position = new Vector3((i) * 50, 0, 0);
                TargetMesh.FragmentShader = new Core.Shader.FragmentShader();
                TargetMesh.Shader = new Shader1();
                PBR.AddObject(TargetMesh);
            }

            ImageRefresher.Interval = TimeSpan.FromTicks(10);
            ImageRefresher.Tick += T_Tick1;
            ImageRefresher.Start();
            RenderTargetImage.Width = width; RenderTargetImage.Height = height;

            //using Context context = Context.Create(builder => builder.AllAccelerators());
            //System.Diagnostics.Debug.WriteLine("Context: " + context.ToString());

            //Device d = context.GetPreferredDevice(preferCPU: false);
            //Accelerator a = d.CreateAccelerator(context);
            //a.PrintInformation();
            //a.Dispose();

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
            for(int i = 0; i < PBR.Targets.Count; i++)
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