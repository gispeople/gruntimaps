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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GruntiMaps.ResourceAccess.Storage;

namespace GruntiMaps.ResourceAccess.Local
{
    public class LocalStorage : IStorage
    {
        private readonly string _containerPath;
        public LocalStorage(string storagePath, string containerName)
        {
            _containerPath = Path.Combine(storagePath, containerName);
            Directory.CreateDirectory(_containerPath);
        }

        // the methods are not really asynchronous but this makes it consistent with other providers
        public async Task<string> Store(string fileName, string inputPath)
        {

            var outputPath = Path.Combine(_containerPath, fileName);
            if (File.Exists(inputPath))
            {
                await Task.Run(() =>
                {
                    File.Copy(inputPath, outputPath);
                });
            }
            return outputPath;
        }

        public Task<string> GetMd5(string fileName)
        {
            var filePath = Path.Combine(_containerPath, fileName);
            string hash = null;
            if (File.Exists(filePath))
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        hash = Convert.ToBase64String(md5.ComputeHash(stream));
                    }
                }
            }
            return Task.FromResult(hash);
        }

        public async Task UpdateLocalFile(string fileName, string localPath)
        {
            var inputPath = Path.Combine(_containerPath, fileName);
            if (File.Exists(inputPath))
            {
                await Task.Run(() =>
                {
                    File.Copy(inputPath, localPath);
                });
            }
        }

        public async Task<List<string>> List()
        {
            var result = new List<string>();
            await Task.Run(() =>
            {
                DirectoryInfo di = new DirectoryInfo(_containerPath);
                if (!di.Exists) return;
                result.AddRange(di.GetFiles().Select(file => file.Name));
            });
            return result;
        }

        public async Task<bool> DeleteIfExist(string fileName)
        {
            var filePath = Path.Combine(_containerPath, fileName);
            if (File.Exists(filePath))
            {
                await Task.Run(() =>
                {
                    File.Delete(filePath);
                });
                return true;
            }
            return false;
        }
    }
}