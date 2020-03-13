using System;

namespace Core.Util
{
    public class SSecurity
    {
//#define DELTA 0x9e3779b9
//#define MX (((z>>5^y<<2) + (y>>3^z<<4)) ^ ((sum^y) + (key[(p&3)^e] ^ z)))
        static void btea(uint[] v, int n, uint[] key) {
            uint y, z, sum;
            uint p, rounds, e;
            if (n > 1) {          /* Coding Part */
                rounds = (uint)(6 + 52 / n);
                sum = 0;
                z = v[n - 1];
                do {
                    sum += 0x9e3779b9;// DELTA;
                    e = (sum >> 2) & 3;
                    for (p = 0; p<n - 1; p++) {
                        y = v[p + 1];
                        z = v[p] += (((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (key[(p & 3) ^ e] ^ z)));// MX;
                    }
                    y = v[0];
                    z = v[n - 1] += (((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (key[(p & 3) ^ e] ^ z)));// MX;
                } while (--rounds > 0);
            }
            else if (n < -1) {  /* Decoding Part */
                n = -n;
                rounds = (uint)(6 + 52 / n);
                sum = rounds * 0x9e3779b9;// DELTA;
                y = v[0];
                do {
                    e = (sum >> 2) & 3;
                    for (p = (uint)(n - 1); p>0; p--) {
                        z = v[p - 1];
                        y = v[p] -= (((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (key[(p & 3) ^ e] ^ z)));// MX;
                    }
                    z = v[n - 1];
                    y = v[0] -= (((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (key[(p & 3) ^ e] ^ z)));// MX;
                    sum -= 0x9e3779b9;// DELTA;
                } while (--rounds > 0);
            }
        }

        static uint[] ToUIntArray(byte[] inData)
        {
            uint[] result = new uint[inData.Length >> 2];  //math.floor(/4)
            for (int i = 0, len = result.Length<<2; i < len; i++)
            {
                result[i >> 2] |= (uint)inData[i] << ((i & 3) << 3);
            }
            return result;
        }

        public static bool xxteaEncode(byte[] inData, uint[] key)
        {
            if (inData == null || (inData.Length >> 2) <= 1) return true;
            //
            uint[] result = ToUIntArray(inData);
            btea(result, result.Length, key);
            //copy to inData
            int bIdx = 0;
            for (int i = 0, len = result.Length; i < len; ++i)
            {
                bIdx = i << 2; //*4
                inData[bIdx + 0] = (byte)((result[i] >> 0 ) & 0xff); //(byte)((result[i] >> ((i & 3) << 3)) & 0xff);
                inData[bIdx + 1] = (byte)((result[i] >> 8 ) & 0xff);
                inData[bIdx + 2] = (byte)((result[i] >> 16) & 0xff);
                inData[bIdx + 3] = (byte)((result[i] >> 24) & 0xff);
            }
            return true;
        }
        public static bool xxteaDecode(byte[] inData, uint[] key)
        {
            if (inData == null || (inData.Length >> 2) <= 1) return true;
            //
            uint[] result = ToUIntArray(inData);
            //Core.Unity.Debug.Log("-------"+result.Length, Core.Unity.Debug.LogLevel.Normal);
            btea(result, -result.Length, key);
            //copy to inData
            int bIdx = 0;
            for (int i = 0, len = result.Length; i < len; ++i)
            {
                bIdx = i << 2; //*4
                inData[bIdx + 0] = (byte)((result[i] >> 0) & 0xff); //(byte)((result[i] >> ((i & 3) << 3)) & 0xff);
                inData[bIdx + 1] = (byte)((result[i] >> 8) & 0xff);
                inData[bIdx + 2] = (byte)((result[i] >> 16) & 0xff);
                inData[bIdx + 3] = (byte)((result[i] >> 24) & 0xff);
            }
            //Core.Unity.Debug.Log("-------=============="+inData.Length, Core.Unity.Debug.LogLevel.Normal);
            //for (int i = 0, len = inData.Length; i < len; ++i )
            //{
            //    Core.Unity.Debug.Log(string.Format("{0:x}", inData[i]), Core.Unity.Debug.LogLevel.Normal);
            //}
            return true;
        }

    }
}
