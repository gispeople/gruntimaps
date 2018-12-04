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
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public async Task<bool> GetIfNewer(string fileName, string outputPath)
        {
            var inputPath = Path.Combine(_containerPath, fileName);
            // if the source doesn't exist, not much to do
            if (!File.Exists(inputPath)) return false;
            // if the dest doesn't exist, no need to check length
            if (File.Exists(outputPath))
            {
                var inputAttrs = new FileInfo(inputPath);
                var outputAttrs = new FileInfo(outputPath);
                if (inputAttrs.Length == outputAttrs.Length)
                {
                    return false;
                }
            }
            await Task.Run(() =>
            {
                File.Copy(inputPath, outputPath);
            });
            return true;
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
    }
}