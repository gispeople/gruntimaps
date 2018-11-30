using System;
using System.Threading.Tasks;
using GruntiMaps.Common.Enums;
using GruntiMaps.ResourceAccess.Table;
using Microsoft.Data.Sqlite;

namespace GruntiMaps.ResourceAccess.Local
{
    public class LocalStatusTable : IStatusTable
    {
        private readonly SqliteConnection _queueDatabase;

        public LocalStatusTable(string storagePath, string tableName)
        {
            var builder = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Private,
                DataSource = System.IO.Path.Combine(storagePath, $"{tableName}.table")
            };
            var connStr = builder.ConnectionString;
            _queueDatabase = new SqliteConnection(connStr);
            _queueDatabase.Open();
            const string createStatusesTable = "CREATE TABLE IF NOT EXISTS Statuses(Id NVARCHAR(50) PRIMARY KEY, Status NVARCHAR(50) NOT NULL)";
            new SqliteCommand(createStatusesTable, _queueDatabase).ExecuteNonQuery();
            _queueDatabase.Close();
        }

        public Task<LayerStatus?> GetStatus(string id)
        {
            _queueDatabase.Open();
            const string getRelatedQueueMsg = "SELECT Status FROM Statuses WHERE Id = $Id";
            var getRelatedQueueCmd = new SqliteCommand(getRelatedQueueMsg, _queueDatabase);
            getRelatedQueueCmd.Parameters.AddWithValue("$Id", id);
            var relatedQueueReader = getRelatedQueueCmd.ExecuteReader();
            if (relatedQueueReader.HasRows)
            {
                Enum.TryParse(relatedQueueReader["Status"].ToString(), out LayerStatus status);
                _queueDatabase.Close();
                return Task.FromResult<LayerStatus?>(status);
            }

            _queueDatabase.Close();
            return Task.FromResult<LayerStatus?>(null);
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
            _queueDatabase.Open();
            var cmd = new SqliteCommand(msg, _queueDatabase);
            cmd.Parameters.AddWithValue("$Status", status.ToString());
            cmd.Parameters.AddWithValue("$Id", id);
            cmd.ExecuteScalar();
            _queueDatabase.Close();
        }

        public void Clear()
        {
            _queueDatabase.Open();
            new SqliteCommand("DELETE FROM Statuses", _queueDatabase).ExecuteNonQuery();
            _queueDatabase.Close();
        }
    }
}
