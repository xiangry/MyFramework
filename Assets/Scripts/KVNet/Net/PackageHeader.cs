using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    namespace Net
    {
        public class PackageHeader
        {
            public static int ByteSize = 8;

            public PackageHeader()
            {
                messageId = 0;
                length = Size();
            }

            public virtual int Size() { return ByteSize; }
            public virtual bool CheckSeqNum(uint inSeqNum) { return true; }
            public virtual uint GetSeqNum() { return 0; }
            public virtual uint GetCrcCheck() { return 0; }
            public virtual byte GetCompressType() { return (byte)NCompressType.No; }
            public virtual uint GetSourceLength() { return 0; }
            public virtual bool SetSeqNum(uint inSeq) { return true; }
            public virtual bool SetCrcCheck(uint inCrcCheck) { return true; }
            public virtual bool SetCrcCheck2(uint inCrcCheck) { return true; }
            public virtual bool SetCompressType(byte inType) { return true; }
            public virtual bool SetSourceLength(uint inLength) { return true; }
            public virtual bool SetExtData(UInt32 data) { return true; }
            public virtual UInt32 GetExtData() { return 0; }

            public virtual bool Serial(byte[] dest, int offset)
            {
                if(dest.Length >= Size())
                {
                    byte[] m = BitConverter.GetBytes(messageId);
                    byte[] l = BitConverter.GetBytes(length);
                    Array.Copy(m, 0, dest, offset + 0, 4);
                    Array.Copy(l, 0, dest, offset + 4, 4);
                    return true;
                }
                return false;
            }

            public virtual bool Unserial(byte[] dest, int offset)
            {
                if (dest.Length >= Size())
                {
                    messageId = BitConverter.ToInt32(dest, 0 + offset);
                    length = BitConverter.ToInt32(dest, 4 + offset);
                    return true;
                }
                return false;
            }

            private int length = 8;
            public int Length  //AllDataLength-HeaderDataLength
            {
                get { return length - Size(); }
                set { length = value+ Size(); }
            }

            private int messageId = 0;
            public int MessageId
            {
                get { return messageId; }
                set { messageId = value; }
            }


        }
    }
}
