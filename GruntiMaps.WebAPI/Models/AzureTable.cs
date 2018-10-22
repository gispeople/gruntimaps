﻿using System;
using System.Threading.Tasks;
using GruntiMaps.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace GruntiMaps.Models
{
    public class AzureTable : ITable
    {
        CloudStorageAccount _account { get; }
        CloudTableClient _client { get; }
        CloudTable _table { get; }

        public const string Workspace = "Workspace";
        public AzureTable(Options options, string tableName)
        {
            _account =
                new CloudStorageAccount(
                    new StorageCredentials(options.StorageAccount, options.StorageKey), true);

            _client = _account.CreateCloudTableClient();
            _table = _client.GetTableReference(tableName);
            _table.CreateIfNotExistsAsync();
        }

        public async Task AddQueue(string queueId, string jobId)
        {
            await _table.ExecuteAsync(TableOperation.Insert(new QueueEntity(queueId, jobId)));
        }

        public async Task<JobStatus?> GetJobStatus(string jobId)
        {
            TableQuery<QueueEntity> query = new TableQuery<QueueEntity>().Where(TableQuery.GenerateFilterCondition("JobId", QueryComparisons.Equal, jobId));
            
            TableContinuationToken token = null;

            bool hasRecord = false;
            bool hasQueued = false;
            bool hasFailed = false;
            bool allFinished = true;
            do
            {
                TableQuerySegment<QueueEntity> resultSegment = await _table.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;

                foreach (QueueEntity queue in resultSegment.Results)
                {
                    hasRecord = true;
                    hasQueued |= queue.Status == JobStatus.Queued.ToString();
                    hasFailed |= queue.Status == JobStatus.Failed.ToString();
                    allFinished &= queue.Status == JobStatus.Finished.ToString();
                }
            } while (token != null);

            if (!hasRecord)
            {
                return null;
            }
            if (hasFailed)
            {
                return JobStatus.Failed;
            }
            if (hasQueued)
            {
                return JobStatus.Queued;
            }
            if (allFinished)
            {
                return JobStatus.Finished;
            }
            throw new Exception($"Unexpected status for job: {jobId}");
        }

        public async Task UpdateQueueStatus(string queueId, JobStatus status)
        {
            if (status != JobStatus.Failed && status != JobStatus.Finished)
            {
                throw new Exception($"Queue Status Update only accept {JobStatus.Failed} or {JobStatus.Finished}");
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<QueueEntity>(Workspace, queueId);

            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);

            if (retrievedResult.Result != null)
            {
                var queue = (QueueEntity) retrievedResult.Result;
                queue.Status = status.ToString();
                await _table.ExecuteAsync(TableOperation.Replace(queue));
            }
            else
            {
                throw new Exception();
            }
        }
    }

    public class QueueEntity : TableEntity
    {
        public QueueEntity(string queueId, string jobId)
        {
            PartitionKey = AzureTable.Workspace;
            RowKey = queueId;
            QueueId = queueId;
            JobId = jobId;
            Status = JobStatus.Queued.ToString();
        }

        public QueueEntity() { }

        public string QueueId { get; set; }

        public string JobId { get; set; }

        public string Status { get; set; }
    }
}
