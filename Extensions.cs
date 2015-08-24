using System;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace Byaltek.Azure
{
    public static class Extensions
    {

        /// <summary>
        ///   Compresses a string using gzip compression
        /// </summary>
        /// <returns>A gzip compressed string</returns>
        public static string CompressString(this string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            content = string.Empty;
            if (bytes.Length > 350)
            {
                MemoryStream mso = null;
                using (var msi = new MemoryStream(bytes))
                    try
                    {
                        mso = new MemoryStream();
                        using (var gs = new GZipStream(mso, CompressionMode.Compress, true))
                        {
                            msi.CopyTo(gs);
                        }
                        mso.Position = 0;
                        StreamReader sr = new StreamReader(mso);
                        content = sr.ReadToEnd();
                    }
                    finally
                    {
                        if (mso != null)
                            mso.Dispose();
                    }
            }
            return content;
        }
    }
}
