using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Net
{
    public class PackageHeaderEx : PackageHeader
    {
        private uint seq_num_ = 0;           //序列号
        private uint crc_check_ = 0;         //crc值
        private byte compress_type_ = 0;    //压缩放式
        private uint source_length_ = 0;     //源数据长度
        private uint ext_data_ = 0;         //扩展数据
        public uint crc_check_2_ = 0;      //crc_check_2
        //
        public static int ByteSize = PackageHeader.ByteSize +
            sizeof(uint) + sizeof(uint) + sizeof(byte) + sizeof(uint) + sizeof(uint) + sizeof(uint);

        public PackageHeaderEx() { }

        public override bool Serial(byte[] dest, int offset)
        {
            if (dest.Length >= Size() && base.Serial(dest, offset))
            {
                offset += base.Size();
                byte[] b1 = BitConverter.GetBytes(seq_num_);
                byte[] b2 = BitConverter.GetBytes(crc_check_);
                byte b3 = compress_type_;
                byte[] b4 = BitConverter.GetBytes(source_length_);
                byte[] b5 = BitConverter.GetBytes(ext_data_);
                byte[] b6 = BitConverter.GetBytes(crc_check_2_);
                Array.Copy(b1, 0, dest, offset + 0, 4);
                Array.Copy(b2, 0, dest, offset + 4, 4);
                dest[offset + 8] = b3;
                Array.Copy(b4, 0, dest, offset + 9, 4);
                Array.Copy(b5, 0, dest, offset + 13, 4);
                Array.Copy(b6, 0, dest, offset + 17, 4);
                //
                return true;
            }
            return false;
        }

        public override bool Unserial(byte[] dest, int offset)
        {
            if (dest.Length >= Size() && base.Unserial(dest, offset))
            {
                offset += base.Size();
                seq_num_       = BitConverter.ToUInt32(dest, offset + 0);
                crc_check_     = BitConverter.ToUInt32(dest, offset + 4);
                compress_type_ = dest[offset + 8];
                source_length_ = BitConverter.ToUInt32(dest, offset + 9);
                ext_data_ = BitConverter.ToUInt32(dest, offset + 13);
                crc_check_2_ = BitConverter.ToUInt32(dest, offset + 17);
                return true;
            }
            return false;
        }


        public override int Size() { return ByteSize; }
        public override bool CheckSeqNum(uint inSeqNum) { return seq_num_ == inSeqNum; }
        public override uint GetSeqNum() { return seq_num_; }
        public override uint GetCrcCheck() { return crc_check_; }
        public override byte GetCompressType() { return compress_type_; }
        public override uint GetSourceLength() { return source_length_; }
        public override bool SetSeqNum(uint inSeq) { seq_num_ = inSeq;  return true; }
        public override bool SetCrcCheck(uint inCrcCheck) { crc_check_ = inCrcCheck; return true; }
        public override bool SetCrcCheck2(uint inCrcCheck) { crc_check_2_ = inCrcCheck; return true; }
        public override bool SetCompressType(byte inType) { compress_type_ = inType; return true; }
        public override bool SetSourceLength(uint inLength) { source_length_ = inLength; return true; }
        public override bool SetExtData(UInt32 data) { ext_data_ = data; return true; }
        public override UInt32 GetExtData() { return ext_data_; } 
    }
}
