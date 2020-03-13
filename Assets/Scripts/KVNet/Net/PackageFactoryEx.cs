using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Net
{
    public class PackageFactoryEx : PackageFactory
    {
        public override int HeaderSize()
        {
            return PackageHeaderEx.ByteSize;
        }

        public override PackageHeader CreatePackageHeader(byte[] data, int offset, int length)
        {
            //if (length - offset >= HeaderSize())
            if (length >= HeaderSize())
            {
                PackageHeader head = new PackageHeaderEx();
                if (head.Unserial(data, offset))
                {
                    return head;
                }
            }
            return null;
        }

        public override Package CreatePackage()
        {
            return new PackageEx();
        }
        
    }
}
