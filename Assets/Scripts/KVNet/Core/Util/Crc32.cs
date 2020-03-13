using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Util
{
    public class Crc32
    {
        static uint[] table = null;

        public static uint ComputeChecksum(byte[] bytes)
        {
//             if (bytes == null) return 0;
//             InitTable();
//             uint crc = 0xffffffff;
//             for (int i = 0; i < bytes.Length; ++i)
//             {
//                 byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
//                 crc = (uint)((crc >> 8) ^ table[index]);
//             }
//             return ~crc;
            return ComputeChecksum(bytes, 0, bytes.Length);
        }

        public static uint ComputeChecksum(byte[] bytes, int startIdx, int length)
        {
            if (bytes == null) return 0;
            length = startIdx + length;
            if (bytes.Length < length) return 0;
            InitTable();
            uint crc = 0xffffffff;
            for (int i = startIdx; i < length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ table[index]);
            }
            return ~crc;
        }



        public static byte[] ComputeChecksumBytes(byte[] bytes)
        {
            return BitConverter.GetBytes(ComputeChecksum(bytes));
        }

        public static void InitTable()
        {
            if(table == null)
            {
                uint poly = 0xedb88320;
                table = new uint[256];
                uint temp = 0;
                for (uint i = 0; i < table.Length; ++i)
                {
                    temp = i;
                    for (int j = 8; j > 0; --j)
                    {
                        if ((temp & 1) == 1)
                        {
                            temp = (uint)((temp >> 1) ^ poly);
                        }
                        else
                        {
                            temp >>= 1;
                        }
                    }
                    table[i] = temp;
                }
            }
        }
    }
}
