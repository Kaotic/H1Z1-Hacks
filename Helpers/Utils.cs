using SlimDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CCleanerBiosUpdater.Helpers
{
    public class Utils
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hwnd, out Rectangle lpRect);
        public static Rectangle GetClientRect(IntPtr hWnd)
        {
            Rectangle lpRect = new Rectangle();
            GetClientRect(hWnd, out lpRect);
            return lpRect;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ClientToScreen(IntPtr hwnd, ref Point lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out Structures.CCleaner_Rectangle lpRect);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("dwmapi.dll")]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Structures.CCleaner_Margin pMargins);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwAccess, bool inherit, int pid);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, long lpBaseAddress, [In, Out] byte[] lpBuffer, ulong dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, long lpBaseAddress, double bBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, long dwFlags);

        [DllImport("shlwapi.dll")]
        public static extern int ColorHLSToRGB(int H, int L, int S);

        public static Structures.CCleaner_Rectangle GetWindowRectangle(IntPtr hWnd)
        {
            try
            {
                Structures.CCleaner_Rectangle LowRectangle = new Structures.CCleaner_Rectangle();
                GetWindowRect(hWnd, out LowRectangle);
                return LowRectangle;
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
            return new Structures.CCleaner_Rectangle();
        }

        public static float ReallocateValue(float ValueAmount, float FirstPoint, float PointTo, float SecondPoint, float PointTo2)
        {
            try
            {
                return ((((ValueAmount - FirstPoint) / (PointTo - FirstPoint)) * (PointTo2 - SecondPoint)) + SecondPoint);
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
            return 0.0f;
        }

        public static Vector2 RotateSentPointing(Vector2 RotatingPoint, Vector2 CenterPointPosition, float FloatAngle, bool RadianAngles = false)
        {
            try
            {
                if (!RadianAngles) FloatAngle = (float)(FloatAngle * 0.017453292519943295);
                float Number1 = (float)Math.Cos((double)FloatAngle);
                float Number2 = (float)Math.Sin((double)FloatAngle);
                Vector2 vector = new Vector2((Number1 * (RotatingPoint.X - CenterPointPosition.X)) - (Number2 * (RotatingPoint.Y - CenterPointPosition.Y)), (Number2 * (RotatingPoint.X - CenterPointPosition.X)) + (Number2 * (RotatingPoint.Y - CenterPointPosition.Y)));
                return (vector + CenterPointPosition);
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
            return new Vector2(0, 0);
        }

        public static Vector3 CalculateMatrixAxis(Matrix MatrixCalculate, int IntegerPoint)
        {
            try
            {
                switch (IntegerPoint)
                {
                    case 0: return new Vector3(MatrixCalculate.M11, MatrixCalculate.M12, MatrixCalculate.M13);
                    case 1: return new Vector3(MatrixCalculate.M21, MatrixCalculate.M22, MatrixCalculate.M23);
                    case 2: return new Vector3(MatrixCalculate.M31, MatrixCalculate.M32, MatrixCalculate.M33);
                    case 3: return new Vector3(MatrixCalculate.M41, MatrixCalculate.M42, MatrixCalculate.M43);
                }
                return Vector3.Zero;
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
            return Vector3.Zero;
        }

    }
}
