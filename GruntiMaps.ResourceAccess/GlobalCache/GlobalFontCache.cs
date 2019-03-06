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

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using GruntiMaps.Api.Common.Configuration;
using Microsoft.Extensions.Options;

namespace GruntiMaps.ResourceAccess.GlobalCache
{
    public class GlobalFontCache: IGlobalFontCache
    {
        private readonly string _path;
        private const string DefaultExtension = "pbf";

        public GlobalFontCache(IOptions<PathOptions> pathOptions)
        {
            _path = pathOptions.Value.Fonts;
            Directory.CreateDirectory(_path);
        }


        public string GetFilePath(string face, string range, string extension = DefaultExtension)
        {
            Directory.CreateDirectory(FontFaceDirectory(face));
            return FilePath(face, range, extension);
        }

        public byte[] GetFileContent(string face, string range, string extension = DefaultExtension)
        {
            return File.ReadAllBytes(FilePath(face, range, extension));
        }

        public string GetFileMd5(string face, string range, string extension = DefaultExtension)
        {
            if (FileExists(face, range, extension))
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.Open(FilePath(face, range, extension), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        return Convert.ToBase64String(md5.ComputeHash(stream));
                    }
                }
            }
            return null;
        }

        public bool DeleteIfExist(string face, string range, string extension = DefaultExtension)
        {
            if (FileExists(face, range, extension))
            {
                File.Delete(FilePath(face, range, extension));
                return true;
            }
            return false;
        }

        public bool FileExists(string face, string range, string extension = DefaultExtension)
        {
            return File.Exists(FilePath(face, range, extension));
        }

        public bool FileIsAvailable(string face, string range, string extension = DefaultExtension)
        {
            if (FileExists(face, range, extension))
            {
                FileStream stream = null;
                try
                {
                    stream = File.Open(FilePath(face, range, extension),
                        FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                catch (IOException)
                {
                    return false;
                }
                finally
                {
                    stream?.Close();
                    stream?.Dispose();
                }
                return true;
            }
            return false;
        }

        public string[] ListFontFaces()
        {
            return Directory.GetDirectories(_path).Select(Path.GetFileName).ToArray();
        }

        public string[] ListFontRanges(string face)
        {
            var directory = FontFaceDirectory(face);
            return Directory.Exists(directory)
                ? Directory.GetFiles(directory).Select(Path.GetFileNameWithoutExtension).ToArray()
                : new string[0];
        }

        private string FontFaceDirectory(string face) => Path.Combine(_path, face);

        private string FileName(string range, string extension = DefaultExtension)
            => $"{range}.{extension ?? DefaultExtension}";

        private string FilePath(string face, string range, string extension = null)
            => Path.Combine(FontFaceDirectory(face), FileName(range, extension));
    }
}
