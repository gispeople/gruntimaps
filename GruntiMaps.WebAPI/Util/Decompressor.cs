/*

Copyright 2016, 2017, 2018 GIS People Pty Ltd

This file is part of GruntiMaps.

GruntiMaps is free software: you can redistribute it and/or modify it under 
the terms of the GNU Affero General Public License as published by the Free
Software Foundation, either version 3 of the License, or (at your option) any
later version.

GruntiMaps is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
A PARTICULAR PURPOSE. See the GNU Affero General Public License for more 
details.

You should have received a copy of the GNU Affero General Public License along
with GruntiMaps.  If not, see <https://www.gnu.org/licenses/>.

*/
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
