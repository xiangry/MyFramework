using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    namespace Net
    {
         //todo 需要添加序列化数据。直接把数据序列化到package中
        public class Package
        {
            public virtual bool encode() { return true;}
            public virtual bool decode() { return true;}
            public virtual void SetKey(uint inKey) { }

            //serial
            public byte[] GetBytes()
            {
                if (header != null)
                {
                    byte[] d = null;
                    if (data != null)
                    {
                        d = new byte[header.Size() + data.Length];
                        Array.Copy(data, 0, d, header.Size(), data.Length);
                    }
                    else
                    {
                        d = new byte[header.Size()];
                    }
                    header.Serial(d, 0);
                    return d;
                }
                return null;
            }
            public int AllSize
            {
                get
                {
                    if (header != null)
                    {
                        return header.Size() + header.Length;
                    }
                    else return 0;
                }
            }
            public int MessageID
            {
                get
                {
                    if (header != null)
                        return header.MessageId;
                    else
                        return 0;
                }
                set
                {
                    if (header != null)
                    {
                        header.MessageId = value;
                    }
                    else
                    {
                        Header = new PackageHeader();
                        header.MessageId = value;
                    }
                }
            }
            protected PackageHeader header = null;
            public PackageHeader Header
            {
                get { return header; }
                set
                {
                    if (value != null)
                    {
                        if (data == null)
                        {
                            value.Length = 0;
                        }
                        else
                        {
                            value.Length = data.Length;
                        }
                    }
                    header = value;
                }
            }
            private byte[] data = null;
            public byte[] Data
            {
                get { return data; }
                set
                {
                    if (header != null && value != null)
                    {
                        header.Length = value.Length;
                    }
                    data = value;
                }
            }
            private Client c = null;

            public Client C
            {
                get { return c; }
                set { c = value; }
            } 
        }
    }
}
