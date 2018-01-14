using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CCleanerBiosUpdater.Helpers
{
    public class CCleanerBios
    {
        public Process ProcessUsername;
        private Int32 ProcessIdentification;
        private IntPtr ProcessHandleIdentification;
        private String ProcessClassUsername;

        ~CCleanerBios()
        {
            if (this.ProcessHandleIdentification != IntPtr.Zero)
            {
                Utils.CloseHandle(this.ProcessHandleIdentification);
            }
        }

        public bool AttachProcess(string lpClassName)
        {
            this.ProcessHandleIdentification = Utils.FindWindow(lpClassName, null);
            if (this.ProcessHandleIdentification != IntPtr.Zero)
            {
                this.ProcessClassUsername = lpClassName;
                Utils.GetWindowThreadProcessId(this.ProcessHandleIdentification, out this.ProcessIdentification);
                this.ProcessUsername = Process.GetProcessById(this.ProcessIdentification);
                this.ProcessHandleIdentification = Utils.OpenProcess(0x38, false, this.ProcessIdentification);
                return (this.ProcessHandleIdentification != IntPtr.Zero);
            }
            return false;
        }

        public bool ProcessRunning
        {
            get
            {
                return this.AttachProcess(this.ProcessClassUsername);
            }
        }

        public byte ReadByte(long lpBaseAddress)
        {
            IntPtr ptr; byte[] lpBuffer = new byte[1];
            Utils.ReadProcessMemory(this.ProcessHandleIdentification, lpBaseAddress, lpBuffer, 1L, out ptr);
            return lpBuffer[0];
        }

        public byte[] ReadBytes(long _lpBaseAddress, ulong _Size)
        {
            IntPtr ptr; byte[] lpBuffer = new byte[_Size];
            Utils.ReadProcessMemory(this.ProcessHandleIdentification, _lpBaseAddress, lpBuffer, _Size, out ptr);
            return lpBuffer;
        }

        public float ReadFloat(long _lpBaseAddress)
        {
            IntPtr ptr; byte[] lpBuffer = new byte[4];
            Utils.ReadProcessMemory(this.ProcessHandleIdentification, _lpBaseAddress, lpBuffer, 4L, out ptr);
            return BitConverter.ToSingle(lpBuffer, 0);
        }

        public double ReadDouble(long _lpBaseAddress)
        {
            IntPtr ptr; byte[] lpBuffer = new byte[8];
            Utils.ReadProcessMemory(this.ProcessHandleIdentification, _lpBaseAddress, lpBuffer, 8L, out ptr);
            return BitConverter.ToDouble(lpBuffer, 0);
        }

        public short ReadInt16(long _lpBaseAddress)
        {
            IntPtr ptr; byte[] lpBuffer = new byte[2];
            Utils.ReadProcessMemory(this.ProcessHandleIdentification, _lpBaseAddress, lpBuffer, 2L, out ptr);
            return BitConverter.ToInt16(lpBuffer, 0);
        }

        public ushort ReadUInt16(long lpBaseAddress)
        {
            IntPtr ptr; byte[] lpBuffer = new byte[2];
            Utils.ReadProcessMemory(this.ProcessHandleIdentification, lpBaseAddress, lpBuffer, 2L, out ptr);
            return BitConverter.ToUInt16(lpBuffer, 0);
        }

        public int ReadInt32(long _lpBaseAddress)
        {
            IntPtr ptr; byte[] lpBuffer = new byte[4];
            Utils.ReadProcessMemory(this.ProcessHandleIdentification, _lpBaseAddress, lpBuffer, 4L, out ptr);
            return BitConverter.ToInt32(lpBuffer, 0);
        }

        public uint ReadUInt32(long lpBaseAddress)
        {
            IntPtr ptr; byte[] lpBuffer = new byte[4];
            Utils.ReadProcessMemory(this.ProcessHandleIdentification, lpBaseAddress, lpBuffer, 4L, out ptr);
            return BitConverter.ToUInt32(lpBuffer, 0);
        }

        public long ReadInt64(long _lpBaseAddress)
        {
            IntPtr ptr; byte[] lpBuffer = new byte[8];
            Utils.ReadProcessMemory(this.ProcessHandleIdentification, _lpBaseAddress, lpBuffer, 8L, out ptr);
            return BitConverter.ToInt64(lpBuffer, 0);
        }

        public ulong ReadUInt64(long lpBaseAddress)
        {
            IntPtr ptr; byte[] lpBuffer = new byte[8];
            Utils.ReadProcessMemory(this.ProcessHandleIdentification, lpBaseAddress, lpBuffer, 8L, out ptr);
            return BitConverter.ToUInt64(lpBuffer, 0);
        }

        public Structures.Matrix4 ReadMatrix(long _lpBaseAddress)
        {
            IntPtr ptr; Structures.Matrix4 matrix = new Structures.Matrix4(); byte[] lpBuffer = new byte[0x40];
            Utils.ReadProcessMemory(this.ProcessHandleIdentification, _lpBaseAddress, lpBuffer, 0x40L, out ptr);
            matrix.M11 = BitConverter.ToSingle(lpBuffer, 0x00);
            matrix.M12 = BitConverter.ToSingle(lpBuffer, 0x04);
            matrix.M13 = BitConverter.ToSingle(lpBuffer, 0x08);
            matrix.M14 = BitConverter.ToSingle(lpBuffer, 0x0C);
            matrix.M21 = BitConverter.ToSingle(lpBuffer, 0x10);
            matrix.M22 = BitConverter.ToSingle(lpBuffer, 0x14);
            matrix.M23 = BitConverter.ToSingle(lpBuffer, 0x18);
            matrix.M24 = BitConverter.ToSingle(lpBuffer, 0x1C);
            matrix.M31 = BitConverter.ToSingle(lpBuffer, 0x20);
            matrix.M32 = BitConverter.ToSingle(lpBuffer, 0x24);
            matrix.M33 = BitConverter.ToSingle(lpBuffer, 0x28);
            matrix.M34 = BitConverter.ToSingle(lpBuffer, 0x2C);
            matrix.M41 = BitConverter.ToSingle(lpBuffer, 0x30);
            matrix.M42 = BitConverter.ToSingle(lpBuffer, 0x34);
            matrix.M43 = BitConverter.ToSingle(lpBuffer, 0x38);
            matrix.M44 = BitConverter.ToSingle(lpBuffer, 0x3C);
            return matrix;
        }

        public string ReadString(long lpBaseAddress, int size)
        {
            IntPtr ptr; byte[] lpBuffer = new byte[size];
            Utils.ReadProcessMemory(this.ProcessHandleIdentification, lpBaseAddress, lpBuffer, (ulong)size, out ptr);
            return Encoding.UTF8.GetString(lpBuffer);
        }

        public bool WriteFloat(long lpBaseAddress, float Buffer)
        {
            IntPtr ptr;
            return Utils.WriteProcessMemory(this.ProcessHandleIdentification, lpBaseAddress, Buffer, sizeof(float), out ptr);
        }
    }
}
