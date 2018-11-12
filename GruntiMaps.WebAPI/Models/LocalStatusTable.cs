using System;
using System.Threading.Tasks;
using GruntiMaps.WebAPI.DataContracts;
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
            const string createStatusesTable = "CREATE TABLE IF NOT EXISTS Statuses(Id NVARCHAR(50) PRIMARY KEY, Status NVARCHAR(50) NOT NULL)";
            new SqliteCommand(createStatusesTable, _queueDatabase).ExecuteNonQuery();
        }

        public Task<LayerStatus?> GetStatus(string id)
        {
            const string getRelatedQueueMsg = "SELECT Status FROM Statuses WHERE Id = $Id";
            var getRelatedQueueCmd = new SqliteCommand(getRelatedQueueMsg, _queueDatabase);
            getRelatedQueueCmd.Parameters.AddWithValue("$Id", id);
            var relatedQueueReader = getRelatedQueueCmd.ExecuteReader();
            if (relatedQueueReader.HasRows)
            {
                Enum.TryParse(relatedQueueReader["Status"].ToString(), out LayerStatus status);
                return Task.FromResult<LayerStatus?>(status);
            }
            else
            {
                return Task.FromResult<LayerStatus?>(null);
            }
        }

        public async Task UpdateStatus(string id, LayerStatus status)
        {
            var currentStatus = await GetStatus(id);

            string msg;
            if (!currentStatus.HasValue)
            {
                // create one if status doesn't exist
                msg = "INSERT INTO Statuses (Id, Status) VALUES($Id, $Status)";
            }
            else
            {
                // update it if it exists
                msg = "UPDATE Statuses SET Status = $Status WHERE Id = $Id";
            }
            var cmd = new SqliteCommand(msg, _queueDatabase);
            cmd.Parameters.AddWithValue("$Status", status.ToString());
            cmd.Parameters.AddWithValue("$Id", id);
            cmd.ExecuteScalar();
        }

        public void Clear()
        {
            new SqliteCommand("DELETE FROM Statuses", _queueDatabase).ExecuteNonQuery();
        }
    }
}
