using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Net
{
    public class PackageEx2 : PackageEx
    {
        uint key_;

        public PackageEx2()
        {
            header = new PackageHeaderEx2();
        }
        
        public override bool encode()
        {
            if( base.encode() )
            {
                uint[] tea_key = new uint[4];
                tea_key[0] = key_;
                tea_key[1] = (uint)this.Header.MessageId;
                tea_key[2] = (uint)this.Header.GetSourceLength();
                tea_key[3] = (uint)this.Header.GetCrcCheck();
                if(true == Core.Util.SSecurity.xxteaEncode(this.Data, tea_key))
                {
                    return true;
                }
            }
            return false;
        }
        public override bool decode()
        {
            bool result = false;
            do
            {
                uint[] tea_key = new uint[4];
	            tea_key[0] = key_;
                tea_key[1] = (uint)this.Header.MessageId;
                tea_key[2] = (uint)this.Header.GetSourceLength();
                tea_key[3] = (uint)this.Header.GetCrcCheck();
                //
                //Debug.LogError(string.Format("package_ex2.decode {0} {1} {2} {3}", tea_key[0], tea_key[1], tea_key[2], tea_key[3]));

                if(false == Core.Util.SSecurity.xxteaDecode(this.Data, tea_key))
                {
                    break;
                }
                if(false == base.decode())
                {
                    break;
                }
                result = true;
            } while (false);
            return result;
        }

        public override void SetKey(uint inKey)
        {
            key_ = inKey;
        }

    }
}
