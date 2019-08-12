using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace GruntiMaps.Common.Extensions
{
    public static class UriExtensions
    {
        public static async Task<string> DownloadToLocal(this Uri uri, string folderPath)
        {
            var filePath = Path.Combine(folderPath, Path.GetFileName(uri.AbsolutePath));
            using (var httpClient = new HttpClient())
            using (var downloadStream = await httpClient.GetStreamAsync(uri))
            using (var localStream = File.OpenWrite(filePath))
            {
                await downloadStream.CopyToAsync(localStream);
            }

            return filePath;
        }
    }
}
