using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace Renderer.Core
{
    public enum KeyCode
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        E = 4,
        F = 5,
        G = 6,
        H = 7,
        I = 8,
        J = 9,            
        K = 10,
        L = 11,
        M = 12,
        N = 13,
        O = 14,
        P = 15,
        Q = 16,
        R = 17,
        S = 18,
        T = 19,
        U = 20,
        V = 21,
        W = 22,
        X = 23,
        Y = 24,
        Z = 25,
        Num0,
        Num1,
        Num2,
        Num3,
        Num4,
        Num5,
        Num6,
        Num7,
        Num8,
        Num9,
        Space,
        LeftShift,
        LeftCtrl,
        RightShift,
        RightCtrl,
        ArrowUp,
        ArrowDown,
        ArrowRight,
        ArrowLeft,
        MouseLeft,
        MouseRight,
        MouseScroll,
        MouseMiddle,
        None
    }
    public enum KeyPreset
    {
        WASD,
        WASDQE,
        Arrow
    }
    public static class Input
    {
        static List<KeyCode> NowInputUpKeys;
        static List<KeyCode> NowInputDownKeys;
        static List<KeyCode> NowInputKeys;
        public static void Start() {
            NowInputKeys = new List<KeyCode>();
            NowInputDownKeys = new List<KeyCode>();
            NowInputUpKeys = new List<KeyCode>();
        }
        public static void DebugNowInputKeys()
        {
            string result = string.Empty;
            for(int i = 0; i < NowInputKeys.Count; i++)
            {
                result += $" {NowInputKeys[i]}";
            }
            System.Diagnostics.Debug.WriteLine(result);
        }
        public static void Content_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (NowInputKeys.Contains(VirtualKet2KeyCode(e.Key)) == true)
                NowInputKeys.Remove(VirtualKet2KeyCode(e.Key));


            if (NowInputDownKeys.Contains(VirtualKet2KeyCode(e.Key)) == false)
                NowInputDownKeys.Add(VirtualKet2KeyCode(e.Key));
        }

        public static void Content_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (NowInputKeys.Contains(VirtualKet2KeyCode(e.Key)) == false)
                NowInputKeys.Add(VirtualKet2KeyCode(e.Key));


            if (NowInputKeys.Contains(VirtualKet2KeyCode(e.Key)) == false)
                NowInputUpKeys.Add(VirtualKet2KeyCode(e.Key));
        }

        public static void Update()
        {
            NowInputDownKeys.Clear();
            NowInputUpKeys.Clear();
        }
        public static bool GetKey(KeyCode code)
        {
            return NowInputKeys.Contains(code);
        }
        public static bool GetKeyDown(KeyCode code)
        {
            return NowInputDownKeys.Contains(code);
        }
        public static bool GetKeyUp(KeyCode code)
        {
            return NowInputUpKeys.Contains(code);
        }
        public static float GetNormlaizedRangeInput(KeyCode from, KeyCode to)
        {
            if (Input.GetKey(from) && Input.GetKey(to))
                return 0;
            if (Input.GetKey(from))
                return -1;
            if (Input.GetKey(to))
                return 1;

            return 0;
        }
        public static Vector3 GetDirectionInput3D(KeyPreset preset)
        {
            Vector3 v = new Vector3(0, 0, 0);
            switch (preset)
            {
                case KeyPreset.WASD:
                    throw new Exception("WASD Preset is not allowed with Vector3.");
                case KeyPreset.WASDQE:
                    if (Input.GetKey(KeyCode.Q))
                        v.y = 1;
                    else if (Input.GetKey(KeyCode.E))
                        v.y = -1;

                    if (Input.GetKey(KeyCode.W))
                        v.z = 1;
                    else if (Input.GetKey(KeyCode.S))
                        v.z = -1;

                    if (Input.GetKey(KeyCode.D))
                        v.x = 1;
                    else if (Input.GetKey(KeyCode.A))
                        v.x = -1;
                    return v;
                case KeyPreset.Arrow:
                    throw new Exception("Arrow Preset is not allowed with Vector3.");
            }
            return v;
        }
        public static Vector3 GetDirectionInput3DXZY(KeyPreset preset)
        {
            Vector3 v = new Vector3(0, 0, 0);
            switch (preset)
            {
                case KeyPreset.WASD:
                    throw new Exception("WASD Preset is not allowed with Vector3.");
                case KeyPreset.WASDQE:
                    if (Input.GetKey(KeyCode.W))
                        v.y = 1;
                    else if (Input.GetKey(KeyCode.S))
                        v.y = -1;

                    if (Input.GetKey(KeyCode.Q))
                        v.z = 1;
                    else if (Input.GetKey(KeyCode.E))
                        v.z = -1;

                    if (Input.GetKey(KeyCode.D))
                        v.x = 1;
                    else if (Input.GetKey(KeyCode.A))
                        v.x = -1;
                    return v;
                case KeyPreset.Arrow:
                    throw new Exception("Arrow Preset is not allowed with Vector3.");
            }
            return v;
        }
        public static Vector2 GetDirectionInput2D(KeyPreset preset)
        {
            Vector2 v = new Vector2(0, 0);
            switch (preset)
            {
                case KeyPreset.WASD:
                    if (Input.GetKey(KeyCode.W))
                        v.y = 1;
                    else if (Input.GetKey(KeyCode.S))
                        v.y = -1;
                    if (Input.GetKey(KeyCode.D))
                        v.x = 1;
                    else if (Input.GetKey(KeyCode.A))
                        v.x = -1;
                    return v;
                case KeyPreset.WASDQE:
                    throw new Exception("WASDQE Preset is not allowed with Vector2.");
                case KeyPreset.Arrow:
                    if (Input.GetKey(KeyCode.ArrowUp))
                        v.y = 1;
                    else if (Input.GetKey(KeyCode.ArrowDown))
                        v.y = -1;
                    if (Input.GetKey(KeyCode.ArrowRight))
                        v.x = 1;
                    else if (Input.GetKey(KeyCode.ArrowLeft))
                        v.x = -1;
                    return v;
            }
            return v;
        }
        static KeyCode VirtualKet2KeyCode(VirtualKey key)
        {
            switch (key)
            {
                case VirtualKey.A: return KeyCode.A;
                case VirtualKey.B: return KeyCode.B;
                case VirtualKey.C: return KeyCode.C;
                case VirtualKey.D: return KeyCode.D;
                case VirtualKey.E: return KeyCode.E;
                case VirtualKey.F: return KeyCode.F;
                case VirtualKey.G: return KeyCode.G;
                case VirtualKey.H: return KeyCode.H;
                case VirtualKey.I: return KeyCode.I;
                case VirtualKey.J: return KeyCode.J;
                case VirtualKey.K: return KeyCode.K;
                case VirtualKey.L: return KeyCode.L;
                case VirtualKey.M: return KeyCode.M;
                case VirtualKey.N: return KeyCode.N;
                case VirtualKey.O: return KeyCode.O;
                case VirtualKey.P: return KeyCode.P;
                case VirtualKey.Q: return KeyCode.Q;
                case VirtualKey.R: return KeyCode.R;
                case VirtualKey.S: return KeyCode.S;
                case VirtualKey.T: return KeyCode.T;
                case VirtualKey.U: return KeyCode.U;
                case VirtualKey.V: return KeyCode.V;
                case VirtualKey.W: return KeyCode.W;
                case VirtualKey.X: return KeyCode.X;
                case VirtualKey.Y: return KeyCode.Y;
                case VirtualKey.Z: return KeyCode.Z;
                case VirtualKey.Number0: return KeyCode.Num0;
                case VirtualKey.Number1: return KeyCode.Num1;
                case VirtualKey.Number2: return KeyCode.Num2;
                case VirtualKey.Number3: return KeyCode.Num3;
                case VirtualKey.Number4: return KeyCode.Num4;
                case VirtualKey.Number5: return KeyCode.Num5;
                case VirtualKey.Number6: return KeyCode.Num6;
                case VirtualKey.Number7: return KeyCode.Num7;
                case VirtualKey.Number8: return KeyCode.Num8;
                case VirtualKey.Number9: return KeyCode.Num9;
                case VirtualKey.Space: return KeyCode.Space;
                case VirtualKey.LeftShift: return KeyCode.LeftShift;
                case VirtualKey.RightShift: return KeyCode.RightShift;
                case VirtualKey.LeftControl: return KeyCode.LeftCtrl;
                case VirtualKey.RightControl: return KeyCode.RightCtrl;
                case VirtualKey.Up: return KeyCode.ArrowUp;
                case VirtualKey.Down: return KeyCode.ArrowDown;
                case VirtualKey.Left: return KeyCode.ArrowLeft;
                case VirtualKey.Right: return KeyCode.ArrowRight;
                //default: throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
            return KeyCode.None;
        }
    }
}
