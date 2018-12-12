using System;
using System.IO;
using System.Security.Cryptography;

namespace GruntiMaps.WebAPI.Utils
{
    public static class HashCalculator
    {
        public static string GetLocalFileMd5(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        return Convert.ToBase64String(md5.ComputeHash(stream));
                    }
                }
            }
            return null;
        }
    }
}
