using System.IO;
using System.IO.Compression;

namespace GruntiMaps.WebAPI.Util
{
    public static class Decompressor
    {
        public static byte[] Decompress(byte[] zLibCompressedBuffer)
        {
            byte[] resBuffer;

            if (zLibCompressedBuffer.Length <= 1)
                return zLibCompressedBuffer;

            var mInStream = new MemoryStream(zLibCompressedBuffer);
            var mOutStream = new MemoryStream(zLibCompressedBuffer.Length);
            var infStream = new GZipStream(mInStream, CompressionMode.Decompress);

            mInStream.Position = 0;

            try
            {
                infStream.CopyTo(mOutStream);

                resBuffer = mOutStream.ToArray();
            }
            finally
            {
                infStream.Flush();
                mInStream.Flush();
                mOutStream.Flush();
            }

            return resBuffer;
        }
    }
}
