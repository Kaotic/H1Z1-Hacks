using SlimDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CCleanerBiosUpdater.Helpers
{
    public class Structures
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct CCleaner_Point
        {
            public int X;
            public int Y;

            public CCleaner_Point(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CCleaner_Rectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CCleaner_Margin
        {
            public int Top;
            public int Left;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Matrix4
        {
            public float M11;
            public float M12;
            public float M13;
            public float M14;
            public float M21;
            public float M22;
            public float M23;
            public float M24;
            public float M31;
            public float M32;
            public float M33;
            public float M34;
            public float M41;
            public float M42;
            public float M43;
            public float M44;
        }

        public struct CCleaner_BoundingSize
        {
            public float length;
            public float width;
            public float height;
            public float origin;

            public CCleaner_BoundingSize(float length2, float width2, float height2, float origin2)
            {
                length = length2;
                width = width2;
                height = height2;
                origin = origin2;
            }
        }

        public class CCleaner_Entities
        {
            public long Identification;
            public Int32 Type;
            public String Name;
            public String Weapon;
            public Vector3 Position;
            public Vector3 Head;
            public Vector3 Feet;
            public Color Color;
            public Vector3 Vector;
            public Vector3 Velocity;
            public Vector2 Screen;
            public float Distance;
            public float Pitch;
            public float Yaw;
            public float Speed;
            public long NumberID;
            public uint Health;
            public long Stance;
            public Byte IsAlive;
            public bool IsMoving;
            public bool MovementCompleted;
        }
    }
}
