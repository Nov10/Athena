using Athena.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace Athena.Engine.Core
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
        Arrow
    }
    public enum KeyPreset3D
    {
        WASDQE,
    }
    /// <summary>
    /// **이 클래스는 환경에 의존합니다.**
    /// </summary>
    public static class Input
    {
        static List<KeyCode> NowInputUpKeys;
        static List<KeyCode> NowInputDownKeys;
        static List<KeyCode> NowInputKeys;
        public static void Start()
        {
            NowInputKeys = new List<KeyCode>();
            NowInputDownKeys = new List<KeyCode>();
            NowInputUpKeys = new List<KeyCode>();
        }
        public static void Update()
        {
            NowInputDownKeys.Clear();
            NowInputUpKeys.Clear();
        }
        public static void DebugNowInputKeys()
        {
            string result = string.Empty;
            for (int i = 0; i < NowInputKeys.Count; i++)
            {
                result += $" {NowInputKeys[i]}";
            }
            System.Diagnostics.Debug.WriteLine(result);
        }

        /// <summary>
        /// KeyUp 이벤트.
        /// **이 함수는 환경에 의존합니다.**
        /// </summary>
        public static void Content_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (NowInputKeys.Contains(VirtualKey2KeyCode(e.Key)) == true)
                NowInputKeys.Remove(VirtualKey2KeyCode(e.Key));

            if (NowInputDownKeys.Contains(VirtualKey2KeyCode(e.Key)) == false)
                NowInputDownKeys.Add(VirtualKey2KeyCode(e.Key));
        }
        /// <summary>
        /// KeyDown 이벤트.
        /// **이 함수는 환경에 의존합니다.**
        /// </summary>
        public static void Content_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (NowInputKeys.Contains(VirtualKey2KeyCode(e.Key)) == false)
                NowInputKeys.Add(VirtualKey2KeyCode(e.Key));

            if (NowInputKeys.Contains(VirtualKey2KeyCode(e.Key)) == false)
                NowInputUpKeys.Add(VirtualKey2KeyCode(e.Key));
        }


        #region GetKey
        /// <summary>
        /// 키가 현재 입력되고 있는가?
        /// </summary>
        public static bool GetKey(KeyCode code)
        {
            return NowInputKeys.Contains(code);
        }
        /// <summary>
        /// 키가 이번에 입력되었는가?
        /// </summary>
        public static bool GetKeyDown(KeyCode code)
        {
            return NowInputDownKeys.Contains(code);
        }
        /// <summary>
        /// 키가 이번에 입력 해제되었는가?
        /// </summary>
        public static bool GetKeyUp(KeyCode code)
        {
            return NowInputUpKeys.Contains(code);
        }
        /// <summary>
        /// from ~ to의 입력을 -1 ~ 1으로 변환합니다.
        /// <para>from이 입력되면 -1을, to가 입력되면 1을, 동시 또는 아무 키도 입력되지 않으면 0을 반환합니다.</para>
        /// </summary>
        public static float GetNormlaizedRangeInput(KeyCode from, KeyCode to)
        {
            if (GetKey(from) && GetKey(to))
                return 0;
            if (GetKey(from))
                return -1;
            if (GetKey(to))
                return 1;

            return 0;
        }
        #endregion

        #region GetKey - Preset
        /// <summary>
        /// 프리셋에 맞는 3D(Vector3) 입력을 가져옵니다.
        /// </summary>
        public static Vector3 GetDirectionInput3D(KeyPreset3D preset)
        {
            Vector3 v = new Vector3(0, 0, 0);
            switch (preset)
            {
                case KeyPreset3D.WASDQE:
                    v.x = GetNormlaizedRangeInput(KeyCode.A, KeyCode.D);
                    v.y = GetNormlaizedRangeInput(KeyCode.E, KeyCode.Q);
                    v.z = GetNormlaizedRangeInput(KeyCode.S, KeyCode.W);
                    return v;
            }
            return v;
        }
        /// <summary>
        /// 프리셋에 맞는 3D(Vector3) 입력을 가져옵니다. 기본 함수의 Y와 Z를 바꿉니다.
        /// </summary>
        public static Vector3 GetDirectionInput3DXZY(KeyPreset3D preset)
        {
            Vector3 v = GetDirectionInput3D(preset);
            (v.y, v.z) = (v.z, v.y);
            return v;
        }
        /// <summary>
        /// 프리셋에 맞는 2D(Vector2) 입력을 가져옵니다.
        /// </summary>
        public static Vector2 GetDirectionInput2D(KeyPreset preset)
        {
            Vector2 v = new Vector2(0, 0);
            switch (preset)
            {
                case KeyPreset.WASD:
                    v.x = GetNormlaizedRangeInput(KeyCode.A, KeyCode.D);
                    v.y = GetNormlaizedRangeInput(KeyCode.S, KeyCode.W);
                    return v;
                case KeyPreset.Arrow:
                    v.x = GetNormlaizedRangeInput(KeyCode.ArrowLeft, KeyCode.ArrowRight);
                    v.y = GetNormlaizedRangeInput(KeyCode.ArrowDown, KeyCode.ArrowUp);
                    return v;
            }
            return v;
        }
        #endregion

        /// <summary>
        /// **이 함수는 환경에 의존합니다.**
        /// </summary>
        static KeyCode VirtualKey2KeyCode(VirtualKey key)
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
