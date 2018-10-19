using System;
using System.Threading.Tasks;
using GruntiMaps.Interfaces;
using Microsoft.Data.Sqlite;

namespace GruntiMaps.Models
{
    public class LocalTable : ITable
    {
        private readonly SqliteConnection _queueDatabase;

        public LocalTable(Options options, string queueName)
        {
            var builder = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Private,
                DataSource = System.IO.Path.Combine(options.StoragePath, $"{queueName}.table")
            };
            var connStr = builder.ConnectionString;
            _queueDatabase = new SqliteConnection(connStr);
            _queueDatabase.Open();
            string createQueueTable =
                "CREATE TABLE IF NOT EXISTS Queue(Id NVARCHAR(50) PRIMARY KEY, JobId NVARCHAR(50) NOT NULL, Status NVARCHAR(50) NOT NULL)";
            new SqliteCommand(createQueueTable, _queueDatabase).ExecuteNonQuery();
        }

        public Task AddQueue(string queueId, string jobId)
        {

            const string addMsg = "INSERT INTO Queue(Id, JobId, Status) VALUES($QueueId, $JobId, $Status)";
            var addMsgCmd = new SqliteCommand(addMsg, _queueDatabase);
            addMsgCmd.Parameters.AddWithValue("$QueueId", queueId);
            addMsgCmd.Parameters.AddWithValue("$JobId", jobId);
            addMsgCmd.Parameters.AddWithValue("$Status", JobStatus.Queued.ToString());
            addMsgCmd.ExecuteScalar();

            return Task.CompletedTask;
        }

        public async Task<JobStatus?> GetJobStatus(string jobId)
        {
            const string getRelatedQueueMsg = "SELECT * FROM Queue WHERE JobId = $JobId";
            var getRelatedQueueCmd = new SqliteCommand(getRelatedQueueMsg, _queueDatabase);
            getRelatedQueueCmd.Parameters.AddWithValue("$JobId", jobId);
            var relatedQueueReader = getRelatedQueueCmd.ExecuteReader();
            bool hasRecord = false;
            bool hasQueued = false;
            bool hasFailed = false;
            bool allFinished = true;
            while (relatedQueueReader.Read())
            {
                hasRecord = true;
                Enum.TryParse(relatedQueueReader["Status"].ToString(), out JobStatus status);
                hasQueued |= status == JobStatus.Queued;
                hasFailed |= status == JobStatus.Failed;
                allFinished &= status == JobStatus.Finished;
            }
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

        public Task UpdateQueueStatus(string queueId, JobStatus status)
        {
            if (status != JobStatus.Failed && status != JobStatus.Finished)
            {
                throw new Exception($"Queue Status Update only accept {JobStatus.Failed} or {JobStatus.Finished}");
            }

            const string updateMsg = "UPDATE Queue SET Status = $Status WHERE Id = $QueueId";
            var updateMsgCmd = new SqliteCommand(updateMsg, _queueDatabase);
            updateMsgCmd.Parameters.AddWithValue("$Status", status.ToString());
            updateMsgCmd.Parameters.AddWithValue("$QueueId", queueId);
            updateMsgCmd.ExecuteScalar();

            return Task.CompletedTask;
        }
    }
}
