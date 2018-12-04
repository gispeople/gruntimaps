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
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace GruntiMaps.ResourceAccess.Azure
{
    public class AzureStatusTable : IStatusTable
    {
        public const string Workspace = "Workspace"; // this is to be replaced when adding workspace function

        private readonly CloudTable _table;

        public AzureStatusTable(string storageAccount, string storageKey, string tableName)
        {
            var account = new CloudStorageAccount(new StorageCredentials(storageAccount, storageKey), true);
            var client = account.CreateCloudTableClient();

            _table = client.GetTableReference(tableName);
            _table.CreateIfNotExistsAsync();
        }

        public async Task<LayerStatus?> GetStatus(string id)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<StatusEntity>(Workspace, id);

            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);

            if (retrievedResult.Result == null)
            {
                return null;
            }

            Enum.TryParse(((StatusEntity)retrievedResult.Result).Status, out LayerStatus status);
            return status;
        }

        public async Task UpdateStatus(string id, LayerStatus status)
        {

            TableOperation retrieveOperation = TableOperation.Retrieve<StatusEntity>(Workspace, id);

            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);

            if (retrievedResult.Result != null)
            {
                var queue = (StatusEntity)retrievedResult.Result;
                queue.Status = status.ToString();
                await _table.ExecuteAsync(TableOperation.Replace(queue));
            }
            else
            {
                await _table.ExecuteAsync(TableOperation.Insert(new StatusEntity(id)));
            }
        }
    }

    public class StatusEntity : TableEntity
    {
        public StatusEntity(string id)
        {
            PartitionKey = AzureStatusTable.Workspace;
            RowKey = id;
            Id = id;
            Status = LayerStatus.Processing.ToString();
        }

        public StatusEntity() { }

        public string Id { get; set; }

        public string Status { get; set; }
    }
}
