using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
//using zlib;
using UnityEngine;

namespace Core.Resource
{
    public class StreamHandle
    {
        protected Stream m_stream = null;
        protected StreamWriter m_writer = null;
        protected StreamReader m_reader = null;

        public StreamHandle(Stream stream)
        {
            m_stream = stream;

            if(stream.CanWrite)
                m_writer = new StreamWriter(stream);

            if(stream.CanRead)
                m_reader = new StreamReader(stream);
        }

        public virtual int Read(ref bool value)
        {
            if(!m_stream.CanRead)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be read"));
                return 0;
            }

            byte[] buffer = new byte[sizeof(bool)];
            int result = m_stream.Read(buffer, 0, buffer.Length);

            if (result > 0)
                value = BitConverter.ToBoolean(buffer, 0);

            return result;
        }

        public virtual int Read(ref byte value)
        {
            if (!m_stream.CanRead)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be read"));
                return 0;
            }

            byte[] buffer = new byte[sizeof(byte)];
            int result = m_stream.Read(buffer, 0, buffer.Length);

            if (result > 0)
                value = buffer[0];

            return result;
        }

        public virtual int Read(ref short value)
        {
            if (!m_stream.CanRead)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be read"));
                return 0;
            }

            byte[] buffer = new byte[sizeof(short)];
            int result = m_stream.Read(buffer, 0, buffer.Length);

            if(result > 0)
                value = BitConverter.ToInt16(buffer, 0);

            return result;
        }

        public virtual int Read(ref int value)
        {
            if (!m_stream.CanRead)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be read"));
                return 0;
            }

            byte[] buffer = new byte[sizeof(int)];
            int result = m_stream.Read(buffer, 0, buffer.Length);

            if (result > 0)
                value = BitConverter.ToInt32(buffer, 0);

            return result;
        }

        public virtual int Read(ref uint value)
        {
            if (!m_stream.CanRead)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be read"));
                return 0;
            }

            byte[] buffer = new byte[sizeof(uint)];
            int result = m_stream.Read(buffer, 0, buffer.Length);

            if (result > 0)
                value = BitConverter.ToUInt32(buffer, 0);

            return result;
        }

        public virtual int Read(ref ulong value)
        {
            if (!m_stream.CanRead)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be read"));
                return 0;
            }

            byte[] buffer = new byte[sizeof(ulong)];
            int result = m_stream.Read(buffer, 0, buffer.Length);

            if (result > 0)
                value = BitConverter.ToUInt64(buffer, 0);

            return result;
        }


        public virtual int Read(ref float value)
        {
            if (!m_stream.CanRead)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be read"));
                return 0;
            }

            byte[] buffer = new byte[sizeof(float)];
            int result = m_stream.Read(buffer, 0, buffer.Length);

            if (result > 0)
                value = BitConverter.ToSingle(buffer, 0);

            return result;
        }

        public virtual int Read(ref string value)
        {
            if (!m_stream.CanRead)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be read"));
                return 0;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int byteValue = -1;
            char charValue = '\n';

            byteValue = m_stream.ReadByte();
            if (byteValue == -1)
                return 0;

            charValue = (char)byteValue;
            while (charValue != '\n')
            {
                sb.Append(charValue);

                byteValue = m_stream.ReadByte();
                if (byteValue == -1)
                    break;

                charValue = (char)byteValue;
            }

            value = sb.ToString();
            return value.Length;
        }

        public virtual int Read(byte[] buffer)
        {
            if (!m_stream.CanRead)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be read"));
                return 0;
            }

            int result = m_stream.Read(buffer, 0, buffer.Length);
            return result;
        }

        public void ReadLine(ref string value)
        {
            if (!m_stream.CanRead || streamReader == null)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be read"));
            }

            value = streamReader.ReadLine();
        }

        public virtual void Write(bool value)
        {
            if (!m_stream.CanWrite)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be write"));
                return;
            }

            byte[] buffer = BitConverter.GetBytes(value);
            m_stream.Write(buffer, 0, buffer.Length);
        }

        public virtual void Write(int value)
        {
            if (!m_stream.CanWrite)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be write"));
                return;
            }

            byte[] buffer = BitConverter.GetBytes(value);
            m_stream.Write(buffer, 0, buffer.Length);
        }

        public virtual void Write(float value)
        {
            if (!m_stream.CanWrite)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be write"));
                return;
            }

            byte[] buffer = BitConverter.GetBytes(value);
            m_stream.Write(buffer, 0, buffer.Length);
        }

        public virtual void Write(string value)
        {
            if (!m_stream.CanWrite)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be write"));
                return;
            }

            string strval = value + '\n';
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strval);
            m_stream.Write(buffer, 0, buffer.Length);
        }

        public virtual void Write(IntPtr ptr, int size)
        {
            if (!m_stream.CanWrite)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be write"));
                return;
            }

            byte[] buffer = new byte[size];
            Marshal.Copy(ptr, buffer, 0, size);
            m_stream.Write(buffer, 0, buffer.Length);
        }

        public void WriteLine(string value)
        {
            if (!m_stream.CanWrite || streamWriter == null)
            {
                Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be write"));
                return;
            }

            streamWriter.WriteLine(value);
        }

        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[512];
            int len;
            while ((len = input.Read(buffer, 0, 512)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public Stream UnCompressStream()
        {

            //TODO zipLib
            Debug.LogError(string.Format("[StreamHandle Read] error: stream can not be write"));
            
            MemoryStream outStream = new MemoryStream();
//            ZOutputStream outZStream = new ZOutputStream(outStream);
//
//            CopyStream(m_stream, outZStream);
//            outZStream.finish();
//
//            outStream.Position = 0;
            return outStream;
        }

        public void Flush()
        {
            m_stream.Flush();
        }

        public void Close()
        {
            m_stream.Close();
        }

        public long position
        {
            get { return m_stream.Position; }
        }

        public long length
        {
            get { return m_stream.Length; }
        }

        public Stream stream
        {
            get
            {
                return m_stream;
            }
        }

        public StreamReader streamReader
        {
            get
            {
                return m_reader;
            }
        }

        public StreamWriter streamWriter
        {
            get
            {
                return m_writer;
            }
        }
    }
}