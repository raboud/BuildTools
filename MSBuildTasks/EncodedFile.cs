using System;
using System.IO;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RandREng.MsBuildTasks
{
    internal class EncodedFile
    {
        private string path;
        private Encoding encoding;

        public EncodedFile(string path)
        {
            this.path = path;
        }

        public string Read()
        {
            // StreamReader detects the encoding but defaults it to UTF-8 even when it is Ansi.
            string content = String.Empty;
            byte[] header = new byte[3];
            using (FileStream stream = File.Open(this.path, FileMode.Open, FileAccess.Read))
            {
                stream.Read(header, 0, header.Length);
                stream.Position = 0; // Reset
                this.encoding = GetEncoding(header);
                using (StreamReader r = new StreamReader(stream, this.encoding))
                {
                    content = r.ReadToEnd();
                }
            }
            return content;            
        }

        private Encoding GetEncoding(byte[] header)
        {
            if (IsEncoding(header, Encoding.UTF8))
                return Encoding.UTF8;

            if (IsEncoding(header, Encoding.Unicode))
                return Encoding.Unicode;
            
            if (IsEncoding(header, Encoding.BigEndianUnicode))
                return Encoding.BigEndianUnicode;
            
            return Encoding.ASCII;
        }

        private bool IsEncoding(byte[] data, Encoding encoding)
        {
            byte[] preamble = encoding.GetPreamble();
            if (null == preamble)
                return false;

            if (data.Length < preamble.Length)
                return false;

            bool isEncoding = true;
            for (int i = 0; i < preamble.Length; i++)
            {
                if (preamble[i] != data[i])
                {
                    isEncoding = false;
                    break;
                }
            }
            return isEncoding;
        }

        public void Write(string content)
        {
            //DateTime createTime = File.GetCreationTime(this.path);
            //DateTime lastWriteTime = File.GetLastWriteTime(this.path);
            //DateTime lastAccessTime = File.GetLastAccessTime(this.path);
            File.SetAttributes(this.path, FileAttributes.Normal);

            using (StreamWriter r = new StreamWriter(this.path, false, this.encoding))
            {
                r.Write(content);
            }
            //File.SetCreationTime(this.path, createTime);
            //File.SetLastWriteTime(this.path, lastWriteTime);
            //File.SetLastAccessTime(this.path, lastAccessTime);
        }
    }
}
