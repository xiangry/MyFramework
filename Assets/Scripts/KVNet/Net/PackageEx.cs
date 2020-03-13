using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Net
{
    public class PackageEx : Package
    {
        private bool isEncode = false;
        private bool isDecode = false;

        public PackageEx()
        {
            header = new PackageHeaderEx();
        }
        
        public override bool encode()
        {
            while (isEncode == false)
            {
                isEncode = true;
                //
                if (this.Data == null) break;
                //
                uint srcLen = (uint)this.Data.Length;
                byte[] outData = null;
                if (false && srcLen > 100 && Core.Util.Data.CompressZlib(this.Data, out outData))
                {
                    this.Data = outData;
                    this.Header.SetSourceLength(srcLen);
                    this.Header.SetCompressType((byte)NCompressType.Zip);
                }
                uint c32 = Core.Util.Crc32.ComputeChecksum(this.Data);
                this.Header.SetCrcCheck(c32);
                this.header.SetCrcCheck2(c32);
                string output = "";
                //    for (int i = 0; i < this.Data.Length; ++i)
                //    {
                //        output += string.Format(" {0:x00}", this.Data[i]);
                //        if (i % 16 ==0)
                //        {
                //            output += "\n";
                //        }
                //    }
                //    Unity.Debug.Log(string.Format("Send package : msgid:{0}, seq_num: {1}, len:{2}, crc:{3}, data:{4}", this.header.MessageId, this.header.GetSeqNum(), this.Data.Length, c32, output));
                //}
            }
            return true;
        }
        public override bool decode()
        {
            while(isDecode == false)
            {
                isDecode = true;
                //
                uint c32 = Core.Util.Crc32.ComputeChecksum(this.Data);
                //Debug.Log(string.Format("package_ex.crc {0} {1} {2}", c32, this.Header.GetCrcCheck(), this.Header.GetCompressType() == (byte)NCompressType.Zip), Unity.Debug.LogLevel.Normal);
                //Unity.Debug.Log(string.Format("package_ex.crc {0} {1} {2}", c32, this.Header.GetCrcCheck(), this.Header.GetCompressType() == (byte)NCompressType.Zip));
                if (this.Header.GetCrcCheck() != c32)
                    break;
                if (this.Data != null && this.Data.Length > 0 && this.Header.GetCompressType() == (byte)NCompressType.Zip)
                {
                    byte[] outData = null;
                    if (Core.Util.Data.DecompressZlib(this.Data, out outData, this.Header.GetSourceLength()) == false)
                    {
                        Logger.LogError("UncompressZlib failed");
                        break;
                    }
                    this.Data = outData;
                }
                return true;
            }
            return false;
        }


    }
}
