using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Net
{
    public class PackageFactory
    {
        public virtual int HeaderSize()
        {
            return PackageHeader.ByteSize;
        }

        public virtual PackageHeader CreatePackageHeader(byte[] data, int offset, int length)
        {
            #if DEBUG_CORE
            Debug.Log("[PackageFactory CreatePackageHeader] create package header start", Unity.Debug.LogLevel.Socket);
            #endif

            if (length >= HeaderSize())
            {
                PackageHeader head = new PackageHeader();
                if (head.Unserial(data, offset))
                {
                    #if DEBUG_CORE
                    Debug.Log("[PackageFactory CreatePackageHeader] create package header success", Unity.Debug.LogLevel.Socket);
                    #endif

                    return head;
                }
            }

            #if DEBUG_CORE
            Debug.Log("[PackageFactory CreatePackageHeader] create package header failed", Unity.Debug.LogLevel.Socket);
            #endif

            return null;
        }

        public virtual Package CreatePackage()
        {
            return new Package();
        }


        //package unserial
        public Package CreatePackage(byte[] data, int offset, int length)
        {
            #if DEBUG_CORE
            Debug.Log("[PackageFactory CreatePackage] create package start, length:" + length.ToString(), Unity.Debug.LogLevel.Socket);
            #endif

            if (data.Length >= (offset + length))
            {
                PackageHeader header = this.CreatePackageHeader(data, offset, length);
                if (header != null && (length - HeaderSize()) >= header.Length)
                {
                    int dataLen = header.Length;

                    #if DEBUG_CORE
                    Debug.Log("[PackageFactory CreatePackage] create package success, message id: " + header.MessageId.ToString() + " body length:" + header.Length.ToString(), Unity.Debug.LogLevel.Socket);
                    #endif

                    Package p = this.CreatePackage();
                    p.Header = header;
                    p.Data = new byte[dataLen];  //必须提交取出header.Length,因在把其设置到package时，它的Length会被旧data重写
                    Array.Copy(data, offset+HeaderSize(), p.Data, 0, header.Length);
                    return p;
                }
            }

            #if DEBUG_CORE
            Debug.Log("[PackageFactory CreatePackage] create package failed", Unity.Debug.LogLevel.Socket);
            #endif

            return null;
        }

    }
}
