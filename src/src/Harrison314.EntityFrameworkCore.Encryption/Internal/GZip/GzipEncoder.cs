using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace Harrison314.EntityFrameworkCore.Encryption.Internal.GZip
{
    internal static class GzipEncoder
    {
        public static byte[] Compress(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            using MemoryStream outputMemoryStream = new MemoryStream(data.Length);
            using GZipStream compressionStream = new GZipStream(outputMemoryStream, CompressionLevel.Optimal);
            compressionStream.Write(data);
            compressionStream.Flush();

            return outputMemoryStream.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            using MemoryStream inputMemoryStream = new MemoryStream(data);
            using MemoryStream outputMemoryStream = new MemoryStream(data.Length);
            using GZipStream decompressStream = new GZipStream(inputMemoryStream, System.IO.Compression.CompressionMode.Decompress);
            decompressStream.CopyTo(outputMemoryStream);

            return outputMemoryStream.ToArray();
        }
    }
}
