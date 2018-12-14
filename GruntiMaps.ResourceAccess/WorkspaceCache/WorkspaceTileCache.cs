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
using GruntiMaps.Api.Common.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace GruntiMaps.ResourceAccess.WorkspaceCache
{
    public class WorkspaceTileCache : WorkspaceCache, IWorkspaceTileCache
    {
        private const string DefaultExtension = "mbtiles";
        public WorkspaceTileCache(IOptions<PathOptions> pathOptions) 
            : base(pathOptions.Value.Tiles, DefaultExtension)
        {
        }

        public bool FileIsValidMbTile(string workspaceId, string id, string extension = null)
        {
            try
            {
                var builder = new SqliteConnectionStringBuilder
                {
                    Mode = SqliteOpenMode.ReadOnly,
                    Cache = SqliteCacheMode.Shared,
                    DataSource = FilePath(workspaceId, id, extension)
                };
                var connStr = builder.ConnectionString;
                var count = 0;
                using (var dbConnection = new SqliteConnection(connStr))
                {
                    dbConnection.Open();
                    using (var cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = "select count(*) from sqlite_master where type='table' and name in ('metadata','tiles')";
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            count = Convert.ToInt32(result);
                        }
                    }
                    dbConnection.Close();
                    if (count == 2) return true;
                }
            }
            catch
            {
                // ignore
            }
            return false;
        }
    }
}
