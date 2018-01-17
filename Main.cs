using System;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;
using SlimDX;
using System.ServiceProcess;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using CCleanerBiosUpdater.Helpers;

//Numpad 8 = Show Vehicles
//Numpad 9 = Show Weapons
//Numpad 7 = Show Debug
//Numpad 5 = Change 2D Box Alpha
//Numpad 3 = Show ESP
//Y = Change predic method
//U = Change shoot point

namespace CCleanerBiosUpdater
{
    public partial class Main : Form
    {
        private delegate void AsyncWrite(String Text);
        private delegate void AsyncClear();
        public static Boolean ProgramRunning = false;
        private static Int64 CCleaner_OffGame = 0x14417CFA0;
        private Int64 CCleaner_OffGraphics = 0x14417DD38;
        private static IntPtr GameHandle;
        private Structures.CCleaner_Margin GameMargin;
        private static CCleanerBios CCleanerBiosReader = new CCleanerBios();
        public Structures.CCleaner_Rectangle GameRectangle;
        public Structures.CCleaner_Point GameSize;
        public Structures.CCleaner_Point GameCenter;
        public static Boolean TextSillohuette = true;
        public static Structures.CCleaner_Point PositionText;
        public static Structures.CCleaner_Entities LocalPlayer;
        public static Structures.CCleaner_Entities EntityTarget = null;
        public static Structures.CCleaner_Entities StickEnemy = null;
        public static Matrix GameMatrix;
        float AimbotFOV = 20f;
        public int CCleaner_AimMethodPredic = 0; //0 no aim - 1 aim, other for local
        public int CCleaner_AimMethodFOV = 0; //0 default, 1 /+0.35
        private static SlimDX.Direct3D9.Device DxD_Device;
        private static SlimDX.Direct3D9.Sprite DxD_Sprite;
        private static SlimDX.Direct3D9.Line DxD_Line;
        private static SlimDX.Direct3D9.Font DxD_Font;
        private static SlimDX.Direct3D9.Font DxD_Font2;

        public string[] CCleaner_Friends = new string[] { "XX", "XXX", "XXXX", "XXXXX", "XXXXXX" }; //Your friend list
        public Color CCleaner_Color2DBox = Color.Red; //2DBox Color
        public int CCleaner_Alpha2DBox = 255; //2DBox Alpha color

        #region Main Events

        public Main()
        {
            InitializeComponents();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                Utils.SetWindowLong(this.Handle, -20, (IntPtr)((Utils.GetWindowLong(this.Handle, -20) ^ 0x80000) ^ 0x20));
                Utils.SetLayeredWindowAttributes(this.Handle, 0, 0xff, 2);

                PresentParameters WindowParameters = new SlimDX.Direct3D9.PresentParameters();
                WindowParameters.Windowed = true;
                WindowParameters.SwapEffect = SwapEffect.Discard;
                WindowParameters.BackBufferFormat = Format.A8R8G8B8;
                WindowParameters.BackBufferHeight = this.Height;
                WindowParameters.BackBufferWidth = this.Width;
                WindowParameters.PresentationInterval = PresentInterval.One;

                DxD_Device = new SlimDX.Direct3D9.Device(new Direct3D(), 0, DeviceType.Hardware, this.Handle, CreateFlags.HardwareVertexProcessing, new PresentParameters[] { WindowParameters });
                DxD_Sprite = new SlimDX.Direct3D9.Sprite(DxD_Device);
                DxD_Line = new SlimDX.Direct3D9.Line(DxD_Device);
                DxD_Font = new SlimDX.Direct3D9.Font(DxD_Device, new System.Drawing.Font("Tahoma", 8f, FontStyle.Bold));
                DxD_Font2 = new SlimDX.Direct3D9.Font(DxD_Device, new System.Drawing.Font("Tahoma", 10f, FontStyle.Bold));
                if (CCleanerBiosReader.AttachProcess("H1Z1 PlayClient (Live)") == false) { Application.Exit(); return; }

                System.IO.File.Create(@"D:/Jeux/SteamLibrary/steamapps/common/H1Z1 King of the Kill/wws_crashreport_uploader.exe");

                GameHandle = CCleanerBiosReader.ProcessUsername.MainWindowHandle;
                Utils.SetForegroundWindow(GameHandle);

                DxD_Device.SetRenderState(RenderState.ZEnable, false);
                DxD_Device.SetRenderState(RenderState.Lighting, false);
                DxD_Device.SetRenderState<Cull>(RenderState.CullMode, Cull.None);

                Thread DirectX_Thread = new Thread(new ThreadStart(DrawThread));
                DirectX_Thread.IsBackground = true;
                DirectX_Thread.Start();

                Thread Utils_Thread = new Thread(new ThreadStart(UtilsThread));
                Utils_Thread.IsBackground = true;
                Utils_Thread.Start();
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
                Application.Exit();
            }
        }

        private void Main_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                this.GameMargin.Top = 0;
                this.GameMargin.Left = 0;
                this.GameMargin.Right = this.Width;
                this.GameMargin.Bottom = this.Height;
                Utils.DwmExtendFrameIntoClientArea(this.Handle, ref this.GameMargin);
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
        }
        #endregion

        #region Draw & Utils
        private bool SendToScreen(Vector3 Vector1, out Vector3 ScreenVector)
        {
            try
            {
                ScreenVector = Vector3.Zero;
                float w = Vector3.Dot(Utils.CalculateMatrixAxis(GameMatrix, 3), Vector1) + GameMatrix.M44;
                if (w < 0.098f) return false;
                float x = Vector3.Dot(Utils.CalculateMatrixAxis(GameMatrix, 0), Vector1) + GameMatrix.M14;
                float y = Vector3.Dot(Utils.CalculateMatrixAxis(GameMatrix, 1), Vector1) + GameMatrix.M24;
                ScreenVector.X = ((((GameSize.X) / 2) * (1f + (x / w))) + this.GameRectangle.Left);
                ScreenVector.Y = ((((GameSize.Y) / 2) * (1f - (y / w))) + this.GameRectangle.Top);
                return true;
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
            ScreenVector = Vector3.Zero;
            return false;
        }

        private bool SendToScreen2D(Vector3 Vector1, out Vector2 ScreenVector)
        {
            try
            {
                ScreenVector = Vector2.Zero;
                float w = Vector3.Dot(Utils.CalculateMatrixAxis(GameMatrix, 3), Vector1) + GameMatrix.M44;
                if (w < 0.098f) return false;
                float x = Vector3.Dot(Utils.CalculateMatrixAxis(GameMatrix, 0), Vector1) + GameMatrix.M14;
                float y = Vector3.Dot(Utils.CalculateMatrixAxis(GameMatrix, 1), Vector1) + GameMatrix.M24;
                ScreenVector.X = ((((GameSize.X) / 2) * (1f + (x / w))) + this.GameRectangle.Left);
                ScreenVector.Y = ((((GameSize.Y) / 2) * (1f - (y / w))) + this.GameRectangle.Top);
                return true;
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
            ScreenVector = Vector2.Zero;
            return false;
        }

        private void ReadGameMatrix()
        {
            try
            {
                long Number1 = CCleanerBiosReader.ReadInt64(CCleaner_OffGraphics);
                long Number2 = CCleanerBiosReader.ReadInt64(Number1 + 0x48);
                long Number3 = CCleanerBiosReader.ReadInt64(Number2 + 0x30);
                Structures.Matrix4 MatrixDefine = CCleanerBiosReader.ReadMatrix(Number3 + 0x1B0);
                Matrix MatrixDefine2 = new Matrix();
                MatrixDefine2.M11 = MatrixDefine.M11;
                MatrixDefine2.M12 = MatrixDefine.M12;
                MatrixDefine2.M13 = MatrixDefine.M13;
                MatrixDefine2.M14 = MatrixDefine.M14;
                MatrixDefine2.M21 = MatrixDefine.M21;
                MatrixDefine2.M22 = MatrixDefine.M22;
                MatrixDefine2.M23 = MatrixDefine.M23;
                MatrixDefine2.M24 = MatrixDefine.M24;
                MatrixDefine2.M31 = MatrixDefine.M31;
                MatrixDefine2.M32 = MatrixDefine.M32;
                MatrixDefine2.M33 = MatrixDefine.M33;
                MatrixDefine2.M34 = MatrixDefine.M34;
                MatrixDefine2.M41 = MatrixDefine.M41;
                MatrixDefine2.M42 = MatrixDefine.M42;
                MatrixDefine2.M43 = MatrixDefine.M43;
                MatrixDefine2.M44 = MatrixDefine.M44;
                Matrix.Transpose(ref MatrixDefine2, out MatrixDefine2);
                MatrixDefine2.M21 *= -1f;
                MatrixDefine2.M22 *= -1f;
                MatrixDefine2.M23 *= -1f;
                MatrixDefine2.M24 *= -1f;
                GameMatrix = MatrixDefine2;
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
        }

        public static void DrawColouredBounding(float PointX, float PointY, float WidthAmount, float HeightAmount, Color Color, int AlphaAmount = 255)
        {
            try
            {
                Vector2[] VertexListing = new Vector2[2];
                DxD_Line.GLLines = false;
                DxD_Line.Antialias = true;
                DxD_Line.Width = WidthAmount;
                VertexListing[0].X = PointX + (WidthAmount / 2f);
                VertexListing[0].Y = PointY;
                VertexListing[1].X = PointX + (WidthAmount / 2f);
                VertexListing[1].Y = PointY + HeightAmount;
                DxD_Line.Begin();
                DxD_Line.Draw(VertexListing, Color.FromArgb(AlphaAmount, Color.R, Color.G, Color.B));
                DxD_Line.End();
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
        }

        public static void DrawSimpleLine(float PointX, float PointY, float PointX_2, float PointY_2, float WidthAmount, Color Color)
        {
            try
            {
                Vector2[] vertexList = new Vector2[] { new Vector2(PointX, PointY), new Vector2(PointX_2, PointY_2) };
                DxD_Line.GLLines = false;
                DxD_Line.Width = WidthAmount;
                DxD_Line.Antialias = true;
                DxD_Line.Begin();
                DxD_Line.Draw(vertexList, Color);
                DxD_Line.End();
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
        }

        public static void DrawStrictText(string TextString, int PointX, int PointY, Color color, bool CenterPoint = false)
        {
            try
            {
                int offset = CenterPoint ? (TextString.Length * 5) / 2 : 0;
                if (TextSillohuette)
                {
                    DxD_Font.DrawString(null, TextString, PointX - offset + 1, PointY + 1, (Color4)Color.Black);
                    DxD_Font.DrawString(null, TextString, PointX - offset - 1, PointY - 1, (Color4)Color.Black);
                }
                DxD_Font.DrawString(null, TextString, PointX - offset, PointY, (Color4)color);
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
        }

        public static void DrawStrictText(string TextString, ref Structures.CCleaner_Point PointThere, Color Color)
        {
            try
            {
                DxD_Font.DrawString(null, TextString, PointThere.X, PointThere.Y, (Color4)Color); PointThere.Y += 15;
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
        }

        public static void DrawStrictText2(string TextString, int PointX, int PointY, Color color, bool CenterPoint = false)
        {
            try
            {
                int offset = CenterPoint ? (TextString.Length * 5) / 2 : 0;
                DxD_Font2.DrawString(null, TextString, PointX - offset, PointY, (Color4)color);
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
        }

        public static void DrawStrictText2(string TextString, ref Structures.CCleaner_Point PointThere, Color Color)
        {
            try
            {
                if (TextSillohuette) DxD_Font2.DrawString(null, TextString, PointThere.X + 1, PointThere.Y + 1, (Color4)Color.Black);
                DxD_Font2.DrawString(null, TextString, PointThere.X, PointThere.Y, (Color4)Color); PointThere.Y += 15;
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
        }

        public static void DrawBoundingBox2D(float x, float y, float w, float h, float thickness, Color color, int alpha = 255)
        {
            try
            {
                Vector2[] vertices =
                {
                new Vector2(x, y),
                new Vector2(x + w, y),
                new Vector2(x + w, y + h),
                new Vector2(x, y + h),
                new Vector2(x, y)
            };
                Main.DxD_Line.Antialias = true;
                Main.DxD_Line.GLLines = false;
                Main.DxD_Line.Width = thickness;
                Main.DxD_Line.Begin();
                Main.DxD_Line.Draw(vertices, Color.FromArgb(alpha, color.R, color.G, color.B));
                Main.DxD_Line.End();
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
        }
        

        float HeightAmount;
        public void CalculateToScreen(Structures.CCleaner_Entities PlayerStruct, Vector3 PlayerHead, Vector3 PlayerPosition, string NameString, uint HealthValue, bool DistanceValue, bool BoundsEnable, float BoxHeight, float YawValue, float PitchValue, bool NotPlayer, int TypeValue)
        {
            try
            {
                string TextNameDistance = null;
                string TextName = null;
                string TextDistance = null;

                Vector3 PlayerDestination = Vector3.Zero;
                this.SendToScreen(PlayerPosition, out PlayerDestination);

                Vector2 PlayerDestinationVector2 = Vector2.Zero;
                this.SendToScreen2D(PlayerPosition, out PlayerDestinationVector2);

                double DistanceX = PlayerPosition.X - Main.LocalPlayer.Position.X;
                double DistanceY = PlayerPosition.Z - Main.LocalPlayer.Position.Z;
                double DistanceZ = PlayerPosition.Y - Main.LocalPlayer.Position.Y;
                double DistanceCalculated = Math.Sqrt(((DistanceX * DistanceX) + (DistanceY * DistanceY)) + (DistanceZ * DistanceZ));

                if (PlayerDestination.Y > 0f && PlayerDestination.X > 0f && PlayerDestination.Y >= this.GameRectangle.Top + 20 && PlayerDestination.X >= this.GameRectangle.Left && PlayerDestination.X <= this.GameRectangle.Right && PlayerDestination.Y <= this.GameRectangle.Bottom)
                {
                    TextNameDistance = "" + NameString + " [" + Math.Round(DistanceCalculated).ToString() + "m]";
                    TextDistance = "\n [" + Math.Round(DistanceCalculated).ToString() + "m]";
                    TextName = NameString;

                    if (BoundsEnable && NotPlayer == false)
                    {
                        Structures.CCleaner_BoundingSize BoxParenthesis = new Structures.CCleaner_BoundingSize(0.5f, 0.6f, 1.7f, 0.2f);
                        double Number5 = PlayerPosition.Z + ((1.0 * Math.Cos((double)PitchValue)) * Math.Cos((double)YawValue));
                        double Number6 = PlayerPosition.X + ((1.0 * Math.Cos((double)PitchValue)) * Math.Sin((double)YawValue));
                        double Number7 = PlayerPosition.Y + (1.0 * Math.Sin((double)PitchValue));
                        Vector3 CalculateVector = new Vector3((float)Number6, (float)Number7, (float)Number5);
                        Vector3 FeetPosition = CalculateVector;
                        FeetPosition.Y += BoxParenthesis.height;
                        Vector2 HeadVector2 = Vector2.Zero;
                        SendToScreen2D(FeetPosition, out HeadVector2);
                        HeightAmount = Vector2.Distance(HeadVector2, PlayerDestinationVector2);
                        Vector3 Position0 = new Vector3(BoxParenthesis.origin, BoxParenthesis.height / 2, -BoxParenthesis.width / 2);
                        Vector3 Position01 = new Vector3(BoxParenthesis.origin, BoxParenthesis.height / 2, BoxParenthesis.width / 2);
                        Vector3 Position02 = new Vector3(BoxParenthesis.origin - BoxParenthesis.length, BoxParenthesis.height / 2, BoxParenthesis.width / 2);
                        Vector3 Position03 = new Vector3(BoxParenthesis.origin - BoxParenthesis.length, BoxParenthesis.height / 2, -BoxParenthesis.width / 2);
                        Matrix rotM = SlimDX.Matrix.RotationY((float)(YawValue - Math.PI / 2));
                        Position0 = Vector3.TransformCoordinate(Position0, rotM) + FeetPosition;
                        Position01 = Vector3.TransformCoordinate(Position01, rotM) + FeetPosition;
                        Position02 = Vector3.TransformCoordinate(Position02, rotM) + FeetPosition;
                        Position03 = Vector3.TransformCoordinate(Position03, rotM) + FeetPosition;
                        Vector2 ScreenCalculate0 = Vector2.Zero;
                        SendToScreen2D(Position0, out ScreenCalculate0);
                        Vector2 ScreenCalculate01 = Vector2.Zero;
                        SendToScreen2D(Position01, out ScreenCalculate01);
                        Vector2 ScreenCalculate02 = Vector2.Zero;
                        SendToScreen2D(Position02, out ScreenCalculate02);
                        Vector2 ScreenCalculate03 = Vector2.Zero;
                        SendToScreen2D(Position03, out ScreenCalculate03);
                        float[] AcuteX = { ScreenCalculate0.X, ScreenCalculate01.X, ScreenCalculate02.X, ScreenCalculate03.X };
                        float WidthAmount = AcuteX.Max() - AcuteX.Min();
                        DrawBoundingBox2D(PlayerDestinationVector2.X - WidthAmount / 2, PlayerDestinationVector2.Y - (HeightAmount / 1.75f), WidthAmount, HeightAmount, 1.5f, CCleaner_Color2DBox, CCleaner_Alpha2DBox);

                        Vector3 Aim3D = Vector3.Zero;

                        Vector3 HeadAddition = (PlayerStruct.Position + (PlayerStruct.Head) / 2);
                        Vector3 TargetPosition = new Vector3(0, 0, 0);
                        Matrix RotateY = SlimDX.Matrix.RotationY((float)(EntityTarget.Yaw - Math.PI));
                        TargetPosition = Vector3.TransformCoordinate(TargetPosition, RotateY) + HeadAddition;
                        SendToScreen(TargetPosition, out Aim3D);

                        int HealthBoxHeight = (int)Math.Max(2.0f, 100f / (float)DistanceCalculated);
                        HealthBoxHeight = Math.Min(HealthBoxHeight, 10);
                        int HealthBoxWidth = Math.Max(30, (int)(70.0 / Math.Max(DistanceCalculated / 10.0, 0.2)));
                        HealthBoxWidth = Math.Min(HealthBoxWidth, 150);
                        float HealthBoxPositionY = PlayerDestinationVector2.Y - (HeightAmount / 1.2f);
                        int HealthAmount = 100 * Convert.ToInt32(HealthValue) / 100;
                        Color ColorCorrection = ColorTranslator.FromWin32(Utils.ColorHLSToRGB(HealthAmount, 120, 240));
                        DrawSimpleLine((int)Aim3D.X - HealthBoxWidth / 2, HealthBoxPositionY, (int)Aim3D.X + HealthBoxWidth / 2 + 1, HealthBoxPositionY, 2 + HealthBoxHeight, Color.Black);
                        DrawSimpleLine((int)Aim3D.X - HealthBoxWidth / 2 + 1, HealthBoxPositionY, (int)Aim3D.X - HealthBoxWidth / 2 + (int)(HealthBoxWidth * HealthAmount / 100f), HealthBoxPositionY, HealthBoxHeight, ColorCorrection);
                    }


                    if (NotPlayer == true)
                    {
                        if (PlayerPosition.X != 0 && PlayerPosition.Y != 0 && PlayerPosition.Z != 0)
                        {
                            if (TypeValue == 0x11 || TypeValue == 0x72 || TypeValue == 0x76 || TypeValue == 0xCC)
                            {
                                DrawStrictText(TextNameDistance, (int)PlayerDestination.X, (int)PlayerDestination.Y, Color.DodgerBlue, true);
                            }
                            else if (TypeValue == 0x2E || TypeValue == 0xAE || TypeValue == 0xB2 || NameString == "Fuel")
                            {
                                DrawStrictText(TextNameDistance, (int)PlayerDestination.X, (int)PlayerDestination.Y, Color.White, true);
                            }
                            else if (TypeValue == 0x34 || TypeValue == 0x15)
                            {
                                DrawStrictText(TextNameDistance, (int)PlayerDestination.X, (int)PlayerDestination.Y, Color.Yellow, true);
                            }
                            else if (TypeValue == 0x2C)
                            {
                                DrawStrictText(TextNameDistance, (int)PlayerDestination.X, (int)PlayerDestination.Y, Color.MediumPurple, true);
                            }
                        }
                    }
                    else if (NotPlayer == false)
                    {
                        DrawStrictText(TextNameDistance, (int)PlayerDestinationVector2.X, (int)PlayerDestinationVector2.Y + (Convert.ToInt32(HeightAmount) / Convert.ToInt32(1.5)), Color.LightGreen, true);
                    }
                }
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
        }

        public Vector3 GetBoneOffset(long EntitySent)
        {
            try
            {
                long SkeletonActor = CCleanerBiosReader.ReadInt64(EntitySent + 0x5E0);
                long SkeletonStart = CCleanerBiosReader.ReadInt64(SkeletonActor + 0x250);
                long SkeletonInformation = CCleanerBiosReader.ReadInt64(SkeletonStart + 0x50);
                long BoneInformation = CCleanerBiosReader.ReadInt64(SkeletonInformation + 0x28);
                Vector3 VectorHead = new Vector3(CCleanerBiosReader.ReadFloat(BoneInformation + 0x1A0), CCleanerBiosReader.ReadFloat(BoneInformation + 0x1A0 + 0x4), CCleanerBiosReader.ReadFloat(BoneInformation + 0x1A0 + 0x4 + 0x4));
                return VectorHead;
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
            return new Vector3(0,0,0);
        }
        #endregion

        bool ShowDebug = true;
        bool ShowWeapons = false;
        bool ShowVehicles = true;
        bool ShowESP = true;
        bool DoneStick = false;

        public void UtilsThread()
        {
            while (true)
            {
                if (Convert.ToBoolean(Utils.GetAsyncKeyState(Keys.NumPad8) & 0x8000))
                {
                    if (ShowVehicles == false)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        ShowVehicles = true;
                    }
                    else if (ShowVehicles == true)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        ShowVehicles = false;
                    }
                    System.Threading.Thread.Sleep(200);
                }

                if (Convert.ToBoolean(Utils.GetAsyncKeyState(Keys.NumPad9) & 0x8000))
                {
                    if (ShowWeapons == false)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        ShowWeapons = true;
                    }
                    else if (ShowWeapons == true)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        ShowWeapons = false;
                    }
                    System.Threading.Thread.Sleep(200);
                }

                if (Convert.ToBoolean(Utils.GetAsyncKeyState(Keys.NumPad7) & 0x8000))
                {
                    if (ShowDebug == false)
                    {
                        ShowDebug = true;
                    }
                    else if (ShowDebug == true)
                    {
                        ShowDebug = false;
                    }
                    System.Threading.Thread.Sleep(200);
                }

                if (Convert.ToBoolean(Utils.GetAsyncKeyState(Keys.NumPad5) & 0x8000))
                {
                    if (CCleaner_Alpha2DBox == 255)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        CCleaner_Alpha2DBox = 100;
                    }
                    else if (CCleaner_Alpha2DBox == 100)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        CCleaner_Alpha2DBox = 255;
                    }
                    System.Threading.Thread.Sleep(200);
                }

                if (Convert.ToBoolean(Utils.GetAsyncKeyState(Keys.NumPad3) & 0x8000))
                {
                    if (ShowESP == false)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        ShowESP = true;
                    }
                    else if (ShowESP == true)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        ShowESP = false;
                    }
                    System.Threading.Thread.Sleep(200);
                }

                if (Convert.ToBoolean(Utils.GetAsyncKeyState(Keys.Y) & 0x8000))
                {
                    if (CCleaner_AimMethodPredic == 0)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        CCleaner_AimMethodPredic = 1;
                    }
                    else if (CCleaner_AimMethodPredic == 1)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        CCleaner_AimMethodPredic = 2;
                    }
                    else if (CCleaner_AimMethodPredic == 2)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        CCleaner_AimMethodPredic = 3;
                    }
                    else if (CCleaner_AimMethodPredic == 3)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        CCleaner_AimMethodPredic = 0;
                    }
                    System.Threading.Thread.Sleep(200);
                }

                if (Convert.ToBoolean(Utils.GetAsyncKeyState(Keys.U) & 0x8000))
                {
                    if (CCleaner_AimMethodFOV == 0)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        CCleaner_AimMethodFOV = 1;
                    }
                    else if (CCleaner_AimMethodFOV == 1)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        CCleaner_AimMethodFOV = 0;
                    }
                    System.Threading.Thread.Sleep(200);
                }
            }
        }
        public void DrawThread()
        {
            try
            {
                Main.ProgramRunning = true;
                Structures.CCleaner_Rectangle rDone = new Structures.CCleaner_Rectangle();

                while (GameHandle != IntPtr.Zero && Main.ProgramRunning && CCleanerBiosReader.ProcessRunning)
                {
                    Structures.CCleaner_Rectangle rCurrent = Utils.GetWindowRectangle(GameHandle);
                    if (!rCurrent.Equals(rDone))
                        rDone = rCurrent;

                    Rectangle ClientRectangle = Utils.GetClientRect(GameHandle);
                    GameSize.X = ClientRectangle.Right;
                    GameSize.Y = ClientRectangle.Bottom;
                    Point TopLeftPoint = new Point(0, 0);
                    Point BottomRightPoint = new Point(ClientRectangle.Right, ClientRectangle.Bottom);
                    Utils.ClientToScreen(GameHandle, ref TopLeftPoint);
                    Utils.ClientToScreen(GameHandle, ref BottomRightPoint);
                    GameRectangle.Left = TopLeftPoint.X;
                    GameRectangle.Top = TopLeftPoint.Y;
                    GameRectangle.Right = BottomRightPoint.X;
                    GameRectangle.Bottom = BottomRightPoint.Y;
                    GameCenter.X = GameRectangle.Left + (GameSize.X / 2);
                    GameCenter.Y = GameRectangle.Top + (GameSize.Y / 2);
                    ReadGameMatrix();

                    DxD_Device.Clear(ClearFlags.Target, Color.Empty, 1f, 0);
                    DxD_Device.BeginScene();

                    long EntityStructure = CCleanerBiosReader.ReadInt64(CCleaner_OffGame);
                    int EntityCount = CCleanerBiosReader.ReadInt32(EntityStructure + 0x50);
                    long PlayerStructure = CCleanerBiosReader.ReadInt64(EntityStructure + 0x13A0);
                    long PositionOffset = CCleanerBiosReader.ReadInt64(PlayerStructure + 0x308);
                    Main.LocalPlayer = new Structures.CCleaner_Entities();
                    Main.LocalPlayer.Position = new Vector3()
                    {
                        X = CCleanerBiosReader.ReadFloat(PositionOffset + 0x3F0),
                        Y = CCleanerBiosReader.ReadFloat(PositionOffset + 0x3F4),
                        Z = CCleanerBiosReader.ReadFloat(PositionOffset + 0x3F8),
                    };

                    if (Convert.ToBoolean(Utils.GetAsyncKeyState(Keys.NumPad6) & 0x8000))
                    {
                        CCleanerBiosReader.WriteFloat(PlayerStructure + 0x450, 500f);
                        CCleanerBiosReader.WriteFloat(PlayerStructure + 0x454, 500f);
                        CCleanerBiosReader.WriteFloat(PlayerStructure + 0x458, 500f);
                        System.Threading.Thread.Sleep(200);
                    }


                    if (ShowDebug == true)
                    {
                        Main.LocalPlayer.Velocity = new Vector3()
                        {
                            X = CCleanerBiosReader.ReadFloat(PlayerStructure + 0x450),
                            Y = CCleanerBiosReader.ReadFloat(PlayerStructure + 0x454),
                            Z = CCleanerBiosReader.ReadFloat(PlayerStructure + 0x458),
                        };

                        Main.LocalPlayer.Yaw = CCleanerBiosReader.ReadFloat(PlayerStructure + 0x550);
                        Main.LocalPlayer.Pitch = CCleanerBiosReader.ReadFloat(PlayerStructure + 0x19C4);

                        PositionText = new Structures.CCleaner_Point(this.GameRectangle.Left + 15, this.GameRectangle.Top + 80);
                        DrawStrictText2("BASIC HACKS [Version: " + Application.ProductVersion + "]", ref PositionText, Color.Red);

                        //PositionText = new Structures.CCleaner_Point(this.GameRectangle.Left + 15, this.GameRectangle.Top + 100);
                        //DrawStrictText2("VelocityX: " + Main.LocalPlayer.Velocity.X + ", VelocityY: " + Main.LocalPlayer.Velocity.Y + ", VelocityZ: " + Main.LocalPlayer.Velocity.Z, ref PositionText, Color.LightGreen);

                        PositionText = new Structures.CCleaner_Point(this.GameRectangle.Left + 15, this.GameRectangle.Top + 120);
                        DrawStrictText2("X: " + Main.LocalPlayer.Position.X + " | Y: " + Main.LocalPlayer.Position.Y + " | Z: " + Main.LocalPlayer.Position.Z, ref PositionText, Color.LightGreen);

                        //PositionText = new Structures.CCleaner_Point(this.GameRectangle.Left + 15, this.GameRectangle.Top + 140);
                        //DrawStrictText2("PlayerYawA: " + Main.LocalPlayer.Yaw + ", PlayerPitch: " + Main.LocalPlayer.Pitch, ref PositionText, Color.LightGreen);

                        PositionText = new Structures.CCleaner_Point(this.GameRectangle.Left + 15, this.GameRectangle.Top + 140);
                        DrawStrictText2("Method: " + CCleaner_AimMethodPredic + " | " + CCleaner_AimMethodFOV, ref PositionText, Color.OrangeRed);

                        PositionText = new Structures.CCleaner_Point(this.GameRectangle.Left + 15, this.GameRectangle.Top + 160);
                        DrawStrictText2("Entities : " + EntityCount, ref PositionText, Color.LightGreen);

                        if (ShowESP == false)
                        {
                            PositionText = new Structures.CCleaner_Point(this.GameRectangle.Left + 15, this.GameRectangle.Top + 180);
                            DrawStrictText2("WL OFF!", ref PositionText, Color.Red);
                        } else {
                            PositionText = new Structures.CCleaner_Point(this.GameRectangle.Left + 15, this.GameRectangle.Top + 180);
                            DrawStrictText2("WL ON.", ref PositionText, Color.LightGreen);
                        }
                    }

                    long EntityEntry = CCleanerBiosReader.ReadInt64(PlayerStructure + 0x720);
                    long NextEntity = CCleanerBiosReader.ReadInt64(PlayerStructure + 0x748);

                    for (int i = 0; i < EntityCount; i++)
                    {
                        long PositionPlayerPointer = CCleanerBiosReader.ReadInt64(EntityEntry + 0x308);
                        Structures.CCleaner_Entities EntityPlayer = new Structures.CCleaner_Entities();
                        EntityPlayer.Position = new Vector3()
                        {
                            X = CCleanerBiosReader.ReadFloat(PositionPlayerPointer + 0x3F0),
                            Y = CCleanerBiosReader.ReadFloat(PositionPlayerPointer + 0x3F0 + 0x4),
                            Z = CCleanerBiosReader.ReadFloat(PositionPlayerPointer + 0x3F0 + 0x4 + 0x4),
                        };

                        EntityPlayer.Velocity = new Vector3()
                        {
                            X = CCleanerBiosReader.ReadFloat(EntityEntry + 0x450),
                            Y = CCleanerBiosReader.ReadFloat(EntityEntry + 0x454),
                            Z = CCleanerBiosReader.ReadFloat(EntityEntry + 0x458),
                        };

                        EntityPlayer.Head = GetBoneOffset(EntityEntry);
                        EntityPlayer.Head.Y = (EntityPlayer.Head.Y / 2.13f);
                        EntityPlayer.Yaw = CCleanerBiosReader.ReadFloat(EntityEntry + 0x550);
                        EntityPlayer.Pitch = CCleanerBiosReader.ReadFloat(EntityEntry + 0x19C4);
                        long EntityHealthStart = CCleanerBiosReader.ReadInt64(EntityEntry + 0x3DB0);
                        EntityPlayer.Health = CCleanerBiosReader.ReadUInt32(EntityHealthStart + 0xB0) / 100;
                        EntityPlayer.Name = CCleanerBiosReader.ReadString(CCleanerBiosReader.ReadInt64(EntityEntry + 0x7C0), CCleanerBiosReader.ReadInt32(EntityEntry + 0x7C8));
                        EntityPlayer.Type = CCleanerBiosReader.ReadInt32(EntityEntry + 0x420);
                        EntityPlayer.Stance = CCleanerBiosReader.ReadInt32(EntityEntry + 0x984);
                        EntityPlayer.IsAlive = CCleanerBiosReader.ReadByte(EntityEntry + 0x1A18);
                        double DistanceX = EntityPlayer.Position.X - Main.LocalPlayer.Position.X;
                        double DistanceY = EntityPlayer.Position.Z - Main.LocalPlayer.Position.Z;
                        double DistanceZ = EntityPlayer.Position.Y - Main.LocalPlayer.Position.Y;
                        double DistanceCalculated = Math.Sqrt(((DistanceX * DistanceX) + (DistanceY * DistanceY)) + (DistanceZ * DistanceZ));
                        int ConvertedDistance = Convert.ToInt32(DistanceCalculated);
                        EntityPlayer.Distance = ConvertedDistance;

                        if (ShowVehicles)
                        {
                            Structures.CCleaner_Entities EntityVehicle = new Structures.CCleaner_Entities();
                            EntityVehicle.Position = new Vector3()
                            {
                                X = CCleanerBiosReader.ReadFloat(NextEntity + 0x440),
                                Y = CCleanerBiosReader.ReadFloat(NextEntity + 0x444),
                                Z = CCleanerBiosReader.ReadFloat(NextEntity + 0x448),
                            };

                            EntityVehicle.Type = CCleanerBiosReader.ReadInt32(NextEntity + 0x420);
                            EntityVehicle.Name = CCleanerBiosReader.ReadString(CCleanerBiosReader.ReadInt64(NextEntity + 0x7C0), CCleanerBiosReader.ReadInt32(NextEntity + 0x7C8));

                            if (ShowESP)
                                this.CalculateToScreen(EntityVehicle, new Vector3(0, 0, 0), EntityVehicle.Position, EntityVehicle.Name, 0, true, true, 0f, 0, 0, true, EntityVehicle.Type);
                        }

                        if (ShowWeapons)
                        {
                            Structures.CCleaner_Entities EntityItems = new Structures.CCleaner_Entities();
                            EntityItems.Position = new Vector3()
                            {
                                X = CCleanerBiosReader.ReadFloat(NextEntity + 0x1880),
                                Y = CCleanerBiosReader.ReadFloat(NextEntity + 0x1884),
                                Z = CCleanerBiosReader.ReadFloat(NextEntity + 0x1888),
                            };

                            EntityItems.Type = CCleanerBiosReader.ReadInt32(NextEntity + 0x420);
                            EntityItems.Name = CCleanerBiosReader.ReadString(CCleanerBiosReader.ReadInt64(NextEntity + 0x7C0), CCleanerBiosReader.ReadInt32(NextEntity + 0x7C8));

                            if (ShowESP)
                            {
                                if (EntityItems.Name == "ATV" || EntityItems.Name == "PoliceCar" || EntityItems.Name == "PickupTruck" || EntityItems.Name == "Offroader")
                                {
                                }
                                else
                                {
                                    this.CalculateToScreen(EntityItems, new Vector3(0, 0, 0), EntityItems.Position, EntityItems.Name, 0, true, true, 0f, 0, 0, true, EntityItems.Type);
                                }
                            }
                        }

                        NextEntity = CCleanerBiosReader.ReadInt64(NextEntity + 0x748);
                        Vector3 Aim3D = Vector3.Zero;

                        if (!CCleaner_Friends.Contains(EntityPlayer.Name))
                        {
                            if (EntityPlayer.IsAlive == 9)
                            {
                                if (ShowESP)
                                {
                                    this.CalculateToScreen(EntityPlayer, new Vector3(0, 0, 0), EntityPlayer.Position, EntityPlayer.Name, EntityPlayer.Health, true, true, 2f, EntityPlayer.Yaw, EntityPlayer.Pitch, false, EntityPlayer.Type);
                                }
                                Vector3 aimingTo = Vector3.Zero;
                                if (Math.Round(EntityPlayer.Distance) > 200f)
                                {
                                }
                                else if (Convert.ToBoolean(Utils.GetAsyncKeyState(Keys.RButton) & 0x8000))
                                {
                                    if (EntityTarget != EntityPlayer && DoneStick == false)
                                        EntityTarget = EntityPlayer;

                                    if (EntityTarget.Health >= 0)
                                    {
                                        Vector3 TargetPosition = new Vector3(0, 0, 0);
                                        Matrix RotateY = SlimDX.Matrix.RotationY((float)(EntityTarget.Yaw - Math.PI / 2));

                                        if(CCleaner_AimMethodFOV == 1)
                                            RotateY = SlimDX.Matrix.RotationY((float)(EntityTarget.Yaw - Math.PI / 2.35));

                                        Vector3 EnginePrediction = (EntityTarget.Position);
                                        if (CCleaner_AimMethodPredic == 1)
                                        {
                                            if (EntityTarget.Distance < 29)
                                            {
                                                EnginePrediction.X += 0;
                                                EnginePrediction.Y += 0;
                                                EnginePrediction.Z += 0;
                                            }
                                            else if (EntityTarget.Distance < 39)
                                            {
                                                EnginePrediction.X += (EntityTarget.Velocity.X) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 1f);
                                                EnginePrediction.Y += (EntityTarget.Velocity.Y) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 1f);
                                                EnginePrediction.Z += (EntityTarget.Velocity.Z) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 1f);
                                            }
                                            else if (EntityTarget.Distance < 100)
                                            {
                                                EnginePrediction.X += (EntityTarget.Velocity.X) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 3.5f);
                                                EnginePrediction.Y += (EntityTarget.Velocity.Y) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 3.5f);
                                                EnginePrediction.Z += (EntityTarget.Velocity.Z) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 3.5f);
                                            }
                                            else if (EntityTarget.Distance > 99)
                                            {
                                                EnginePrediction.X += (EntityTarget.Velocity.X) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 15f);
                                                EnginePrediction.Y += (EntityTarget.Velocity.Y) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 15f);
                                                EnginePrediction.Z += (EntityTarget.Velocity.Z) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 15f);
                                            }
                                            else if (EntityTarget.Distance > 149)
                                            {
                                                EnginePrediction.X += (EntityTarget.Velocity.X) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 50f);
                                                EnginePrediction.Y += (EntityTarget.Velocity.Y) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 50f);
                                                EnginePrediction.Z += (EntityTarget.Velocity.Z) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 50f);
                                            }

                                            if (EntityTarget.Distance > 49)
                                            {
                                                float BulletCurve = (0.00365f * EntityTarget.Distance);
                                                EnginePrediction.Y += BulletCurve;
                                            }
                                        }else if(CCleaner_AimMethodPredic == 2)
                                        {
                                            EnginePrediction.X += (EntityTarget.Velocity.X) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 1f);
                                            EnginePrediction.Y += (EntityTarget.Velocity.Y) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 1f);
                                            EnginePrediction.Z += (EntityTarget.Velocity.Z) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 1f);
                                        }
                                        else if(CCleaner_AimMethodPredic == 3)
                                        {
                                            EnginePrediction.X += (EntityTarget.Velocity.X) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 0.5f);
                                            EnginePrediction.Y += (EntityTarget.Velocity.Y) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 0.5f);
                                            EnginePrediction.Z += (EntityTarget.Velocity.Z) / EntityTarget.Distance * (EntityTarget.Distance / 10 * 0.5f);
                                        }
                                        

                                        TargetPosition = Vector3.TransformCoordinate(EntityTarget.Head, RotateY) + EnginePrediction;
                                        SendToScreen(TargetPosition, out Aim3D);

                                        if (EntityTarget != null)
                                        {
                                            {
                                                Vector3 PlayerDestination = Vector3.Zero;
                                                int moveOffsetX = (int)(Math.Round(Aim3D.X) - this.GameCenter.X) / 2;
                                                int moveOffsetY = (int)(Math.Round(Aim3D.Y) - this.GameCenter.Y) / 2;
                                                if (Math.Sqrt(moveOffsetX * moveOffsetX + moveOffsetY * moveOffsetY) < AimbotFOV)
                                                {
                                                    if(CCleaner_AimMethodPredic != 0)
                                                        Utils.mouse_event(0x0001, (short)moveOffsetX, (short)moveOffsetY, 0, 0);

                                                }
                                            }
                                        }
                                        else if (EntityTarget == null && SendToScreen(new Vector3(Aim3D.X, Aim3D.Y, Aim3D.Z), out aimingTo))
                                        {
                                            float EntityDistance = Vector2.Distance(new Vector2(aimingTo.X, aimingTo.Y), new Vector2(this.GameCenter.X, this.GameCenter.Y));
                                            if (EntityDistance <= 100f)
                                            {
                                                float moveOffsetX = aimingTo.X - this.GameCenter.X;
                                                float moveOffsetY = aimingTo.Y - this.GameCenter.Y;

                                                if (CCleaner_AimMethodPredic != 0)
                                                    Utils.mouse_event(0x0001, (short)moveOffsetX, (short)moveOffsetY, 0, 0);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        EntityEntry = CCleanerBiosReader.ReadInt64(EntityEntry + 0x720);
                    }
                    DxD_Device.EndScene();
                    DxD_Device.Present();
                }
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
            DxD_Device.Dispose();
            Application.Exit();
        }

        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == 0x312)
                {
                    switch ((int)m.WParam)
                    {
                        case 0:
                            Utils.SetForegroundWindow(CCleanerBiosReader.ProcessUsername.MainWindowHandle);
                            break;

                        case 1:
                            Main.ProgramRunning = false;
                            break;
                    }
                }
                base.WndProc(ref m);
            }
            catch (Exception ExceptionString)
            {
                string ExceptionMessage = Convert.ToString(ExceptionString);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "Main";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
        }
    }
}
