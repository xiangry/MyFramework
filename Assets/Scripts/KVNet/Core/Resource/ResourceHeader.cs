using System;
using System.IO;
using System.Collections;

namespace Core.Resource
{
    public class ResourceHeader
    {
        public enum FILE_TYPE:byte
        {
            ANIMATION = 1,
            UI = 2,
            SCRIPT = 3,
        }

        public enum ZIP_TYPE
        {
            ZIP_NO = 0,
            ZIP_7Z = 1,
            ZIP_GZIP = 2,
            ZIP_ZLIB = 3
        }

        public enum ENCRYPT_TYPE
        {
            ENCRYPT_NO = 0,
            ENCRYPT_BASE64 = 1,
            ENCRYPT_NOR = 2
        }

        public enum CHECK_TYPE
        {
            CHECK_NO = 0,
            CHECK_MD5 = 1,
            CHECK_CRC = 2
        }

        public bool Unserial(Stream ms)
        {
            int result = 0;

            StreamHandle msHandle = new StreamHandle(ms);
            result += msHandle.Read(ref m_headerSize);
            result += msHandle.Read(ref m_herderVersion);
            result += msHandle.Read(ref m_fileSize);
            result += msHandle.Read(ref m_fileType);
            result += msHandle.Read(ref m_encryptType);
            result += msHandle.Read(ref m_zipType);
            result += msHandle.Read(ref m_checkType);
            result += msHandle.Read(m_encryptKey);
            result += msHandle.Read(m_checkCode);

            if (result == HERDER_SIZE)
                return true;

            return false;
        }

        public  short headerSize
        {
            get
            {
                return m_headerSize;
            }
        }

        public short herderVersion
        {
            get
            {
                return m_herderVersion;
            }
        }

        public int fileSize
        {
            get
            {
                return m_fileSize;
            }
        }

        public byte fileType
        {
            get
            {
                return m_fileType;
            }
        }

        public byte encryptType
        {
            get
            {
                return m_encryptType;
            }
        }

        public byte[] encryptKey
        {
            get
            {
                return m_encryptKey;
            }
        }

        private short m_headerSize;
        private short m_herderVersion;
        private int   m_fileSize;
        private byte  m_fileType;
        private byte  m_encryptType;
        private byte  m_zipType;
        private byte  m_checkType;
        private byte[] m_encryptKey = new byte[16];
        private byte[] m_checkCode = new byte[16];

        public const int HERDER_SIZE = 44;
        public const int HEADER_VERSION = 1;
    }
}
