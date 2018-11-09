using System;
using System.Linq;
using System.Threading.Tasks;
using GruntiMaps.WebAPI.DataContracts;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace GruntiMaps.WebAPI.Models
{
    public class AzureStatusTable : IStatusTable
    {
        public const string Workspace = "Workspace"; // this is to be replaced when adding workspace function

        private readonly CloudTable _table;

        public AzureStatusTable(Options options, string tableName)
        {
            var account = new CloudStorageAccount(
                new StorageCredentials(options.StorageAccount, options.StorageKey), true);

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
                var queue = (StatusEntity) retrievedResult.Result;
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
