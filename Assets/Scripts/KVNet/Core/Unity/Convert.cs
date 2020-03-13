using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Core.Unity
{
    public class Convert
    {
        public static uint ReadUint(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            {
                return br.ReadUInt32();
            }
        }
        public static int ReadInt(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            {
                return br.ReadInt32();
            }
        }
        public static byte ReadByte(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            {
                return br.ReadByte();
            }
        }
        public static bool ReadBool(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            {
                return br.ReadBoolean();
            }
        }
        public static Int16 ReadShort(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            {
                return br.ReadInt16();
            }
        }
        public static UInt16 ReadUShort(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            {
                return br.ReadUInt16();
            }
        }

        public static string ReadString(Stream s)
        {
            UInt16 len = ReadUShort(s);
            if (len == 0)
                return String.Empty;
            if (len <= (s.Length - s.Position))
            {
                byte[] d = new byte[len];
                s.Read(d, 0, len);
                return  Encoding.UTF8.GetString(d);
            }
            return string.Empty;
        }
    }
}
