using System;
using System.Threading.Tasks;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.Data.Sqlite;

namespace GruntiMaps.WebAPI.Models
{
    public class LocalStatusTable : IStatusTable
    {
        private readonly SqliteConnection _queueDatabase;

        public LocalStatusTable(Options options, string queueName)
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
            const string createStatusesTable = "CREATE TABLE IF NOT EXISTS Statuses(Id NVARCHAR(50) PRIMARY KEY, JobId NVARCHAR(50) NOT NULL, Status NVARCHAR(50) NOT NULL)";
            new SqliteCommand(createStatusesTable, _queueDatabase).ExecuteNonQuery();
            const string createJobIdIndex = "CREATE INDEX IF NOT EXISTS index_JobId ON Statuses(JobId)";
            new SqliteCommand(createJobIdIndex, _queueDatabase).ExecuteNonQuery();
        }

        public Task AddStatus(string queueId, string jobId)
        {

            const string addMsg = "INSERT INTO Statuses(Id, JobId, Status) VALUES($QueueId, $JobId, $Status)";
            var addMsgCmd = new SqliteCommand(addMsg, _queueDatabase);
            addMsgCmd.Parameters.AddWithValue("$QueueId", queueId);
            addMsgCmd.Parameters.AddWithValue("$JobId", jobId);
            addMsgCmd.Parameters.AddWithValue("$Status", JobStatus.Queued.ToString());
            addMsgCmd.ExecuteScalar();

            return Task.CompletedTask;
        }

        public Task<JobStatus?> GetStatus(string jobId)
        {
            const string getRelatedQueueMsg = "SELECT * FROM Statuses WHERE JobId = $JobId";
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
                return Task.FromResult<JobStatus?>(JobStatus.Failed);
            }
            if (hasQueued)
            {
                return Task.FromResult<JobStatus?>(JobStatus.Queued);
            }
            if (allFinished)
            {
                return Task.FromResult<JobStatus?>(JobStatus.Finished);
            }
            throw new Exception($"Unexpected status for job: {jobId}");
        }

        public Task UpdateStatus(string queueId, JobStatus status)
        {
            if (status != JobStatus.Failed && status != JobStatus.Finished)
            {
                throw new Exception($"Queue Status Update only accept {JobStatus.Failed} or {JobStatus.Finished}");
            }

            const string updateMsg = "UPDATE Statuses SET Status = $Status WHERE Id = $QueueId";
            var updateMsgCmd = new SqliteCommand(updateMsg, _queueDatabase);
            updateMsgCmd.Parameters.AddWithValue("$Status", status.ToString());
            updateMsgCmd.Parameters.AddWithValue("$QueueId", queueId);
            updateMsgCmd.ExecuteScalar();

            return Task.CompletedTask;
        }

        public void Clear()
        {
            new SqliteCommand("DELETE FROM Statuses", _queueDatabase).ExecuteNonQuery();
        }
    }
}
