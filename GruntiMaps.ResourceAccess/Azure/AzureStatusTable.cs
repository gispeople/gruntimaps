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
using System.Threading.Tasks;
using GruntiMaps.Common.Enums;
using GruntiMaps.ResourceAccess.Table;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace GruntiMaps.ResourceAccess.Azure
{
    public class AzureStatusTable : IStatusTable
    {
        private readonly CloudTable _table;

        public AzureStatusTable(string connectionString, string tableName)
        {
            _table = CloudStorageAccount
                .Parse(connectionString)
                .CreateCloudTableClient()
                .GetTableReference(tableName);
            _table.CreateIfNotExistsAsync();
        }

        public async Task<LayerStatus?> GetStatus(string workspaceId, string layerId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<StatusEntity>(workspaceId, layerId);
            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);
            if (retrievedResult.Result == null)
            {
                return null;
            }

            Enum.TryParse(((StatusEntity)retrievedResult.Result).Status, out LayerStatus status);
            return status;
        }

        public async Task UpdateStatus(string workspaceId, string layerId, LayerStatus status)
        {

            TableOperation retrieveOperation = TableOperation.Retrieve<StatusEntity>(workspaceId, layerId);

            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);

            if (retrievedResult.Result != null)
            {
                var queue = (StatusEntity)retrievedResult.Result;
                queue.Status = status.ToString();
                await _table.ExecuteAsync(TableOperation.Replace(queue));
            }
            else
            {
                await _table.ExecuteAsync(TableOperation.Insert(new StatusEntity(workspaceId, layerId)));
            }
        }

        public async Task RemoveStatus(string workspaceId, string layerId)
        {
            try
            {
                await _table.ExecuteAsync(TableOperation.Delete(new DynamicTableEntity(workspaceId, layerId) {ETag = "*"}));
            }
            catch (StorageException e)
            {
                if (e.Message != "Not Found")
                {
                    throw;
                }
            }
            
        }
    }

    public class StatusEntity : TableEntity
    {
        public StatusEntity(string workspaceId, string id)
        {
            PartitionKey = workspaceId;
            RowKey = id;
            Status = LayerStatus.Processing.ToString();
        }

        public StatusEntity() { }

        public string Status { get; set; }
    }
}
