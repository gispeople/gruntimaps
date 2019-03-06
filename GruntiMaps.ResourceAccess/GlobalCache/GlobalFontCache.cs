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
