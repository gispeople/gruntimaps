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

namespace GruntiMaps.ResourceAccess.WorkspaceCache
{
    public abstract class WorkspaceCache : IWorkspaceCache
    {
        private readonly string _path;
        private readonly string _defaultExtention;

        protected WorkspaceCache(string path,
            string defaultExtention)
        {
            _path = path;
            _defaultExtention = defaultExtention;
            Directory.CreateDirectory(path);
        }

        public string GetFilePath(string workspaceId, string id, string extension = null)
        {
            Directory.CreateDirectory(WorkspaceDirectory(workspaceId));
            return FilePath(workspaceId, id, extension);
        }

        public string GetFileMd5(string workspaceId, string id, string extension = null)
        {
            if (FileExists(workspaceId, id, extension))
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(FilePath(workspaceId, id, extension)))
                    {
                        return Convert.ToBase64String(md5.ComputeHash(stream));
                    }
                }
            }
            return null;
        }

        public bool DeleteIfExist(string workspaceId, string id, string extension = null)
        {
            if (FileExists(workspaceId, id, extension))
            {
                File.Delete(FilePath(workspaceId, id, extension));
                return true;
            }
            return false;
        }

        public bool FileExists(string workspaceId, string id, string extension = null)
        {
            return File.Exists(FilePath(workspaceId, id, extension));
        }

        public bool FileIsAvailable(string workspaceId, string id, string extension = null)
        {
            if (FileExists(workspaceId, id, extension))
            {
                FileStream stream = null;
                try
                {
                    stream = File.Open(FilePath(workspaceId, id, extension),
                        FileMode.Open, FileAccess.Read, FileShare.None);
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

        public string[] ListFilePaths(string workspaceId)
        {
            var directory = WorkspaceDirectory(workspaceId);
            return Directory.Exists(directory) ? Directory.GetFiles(directory) : new string[0];
        }

        public string[] ListFileIds(string workspaceId)
        {
            var directory = WorkspaceDirectory(workspaceId);
            return Directory.Exists(directory)
                ? Directory.GetFiles(directory).Select(Path.GetFileNameWithoutExtension).ToArray()
                : new string[0];
        }

        public string[] ListWorkspaces()
        {
            return Directory.GetDirectories(_path).Select(Path.GetFileName).ToArray();
        }

        protected string WorkspaceDirectory(string workspaceId)
            => Path.Combine(_path, workspaceId);

        protected string FileName(string layerId, string extension = null)
            => $"{layerId}.{extension ?? _defaultExtention}";

        protected string FilePath(string workspaceId, string layerId, string extension = null)
            => Path.Combine(WorkspaceDirectory(workspaceId), FileName(layerId, extension));
    }
}
