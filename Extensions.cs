using System;
using System.IO;
using System.Text;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Byaltek.Azure
{
    public static class StringExtensions
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
                using (var msi = new MemoryStream(bytes))
                using (var mso = new MemoryStream())
                {
                    using (var gzip = new GZipStream(mso, CompressionMode.Compress, true))
                    {
                        msi.CopyTo(gzip);
                    }
                    mso.Position = 0;
                    StreamReader sr = new StreamReader(mso);
                    content = sr.ReadToEnd();
                }
            }
            return content;
        }
        
        public static async Task<string> CompressStringAsync(this string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            content = string.Empty;
            if (bytes.Length > 350)
            {
                using (var msi = new MemoryStream(bytes))
                using (var mso = new MemoryStream())
                {
                    using (var gzip = new GZipStream(mso, CompressionMode.Compress, true))
                    {
                        await msi.CopyToAsync(gzip);
                    }
                    // After writing to the MemoryStream, the position will be the size
                    // of the decompressed file, we should reset it back to zero before returning.
                    mso.Position = 0;
                    StreamReader sr = new StreamReader(mso);
                    content = sr.ReadToEnd();
                }
            }
            return content;
        }        
    }
}
