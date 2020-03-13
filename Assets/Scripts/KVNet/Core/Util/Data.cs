using System;
using System.IO;

namespace Core.Util
{
    public class Data
    {
        public static bool CompressZlib(byte[] inData, out byte[] outData)
        {
            MemoryStream output = new MemoryStream();
            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(output, zlib.zlibConst.Z_DEFAULT_COMPRESSION);
            /*
            for (int i = 0, len = inData.Length; i < len; i += 1024)
            {
                outZStream.Write(inData, i, len - i > 1024 ? 1024 : len - i);
            }*/

            outZStream.Write(inData, 0, inData.Length);

            outZStream.Flush();
            outZStream.finish();
            outData = output.ToArray();
            //Core.Unity.Debug.LogError(string.Format("CompressZlib:zzzzzzzz: {0} {1} {2} {3} {4}", inData.Length, outZStream.Length, output.Length, output.Position, outData.Length ));
            return true;
        }

        public static bool DecompressZlib(byte[] inData, out byte[] outData, uint outDataRefLen)
        {
//             MemoryStream output = new MemoryStream();
//             zlib.ZOutputStream outZStream = new zlib.ZOutputStream(output);
//             for (int i = 0, len = inData.Length; i < len; i += 1024)
//             {
//                 outZStream.Write(inData, i, len - i > 1024 ? 1024 : len - i);
//             }
//             outZStream.Flush();
//             outZStream.finish();
//             outData = output.ToArray();
//             //Core.Unity.Debug.LogError(string.Format("DecompressZlib:zzzzzzzz: {0} {1} {2} {3} {4} {5}", inData.Length, outZStream.Length, output.Length, output.Position, outDataRefLen, outData.Length));
//             if (output.Length != outDataRefLen)
//             {
//                 outData = null;
//                 return false;
//             }
//             return true;
            return DecompressZlib(inData, 0, inData.Length, out outData, outDataRefLen);
        }
        
        public static bool DecompressZlib(byte[] inData, int startIndex, int length, out byte[] outData, uint outDataRefLen)
        {
            //length = startIndex + length;
            if (inData.Length < startIndex + length)
            {
                outData = null;
                return false;
            }
            //
            MemoryStream output = new MemoryStream();
            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(output);

            /*
            for (int i = startIndex; i < length; i += 1024)
            {
                outZStream.Write(inData, i, length - i > 1024 ? 1024 : length - i);
            }*/

            outZStream.Write(inData, startIndex, length);

            outZStream.Flush();
            outZStream.finish();
            outData = output.ToArray();
            //Core.Unity.Debug.LogError(string.Format("DecompressZlib:zzzzzzzz: {0} {1} {2} {3} {4} {5}", inData.Length, outZStream.Length, output.Length, output.Position, outDataRefLen, outData.Length));
            if (output.Length != outDataRefLen)
            {
                outData = null;
                return false;
            }
            return true;
        }
    }
}
