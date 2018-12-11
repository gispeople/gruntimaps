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
using GruntiMaps.ResourceAccess.Queue;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace GruntiMaps.ResourceAccess.Local
{
    public class LocalConversionQueue : IConversionQueue
    {
        private readonly SqliteConnection _queueDatabase;
        private readonly int _queueTimeLimit;
        private readonly int _queueEntryTries;

        public LocalConversionQueue(string storagePath, int queueTimeLimit, int queueEntryTries, string queueName)
        {
            _queueTimeLimit = queueTimeLimit;
            _queueEntryTries = queueEntryTries;
            var builder = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared,
                DataSource = System.IO.Path.Combine(storagePath, $"{queueName}.queue")
            };
            var connStr = builder.ConnectionString;
            _queueDatabase = new SqliteConnection(connStr);
            _queueDatabase.Open();
            using (SqliteCommand createQueueTableCmd = new SqliteCommand()) {
                createQueueTableCmd.Connection = _queueDatabase;
                createQueueTableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Queue(" +
                                                  "ID NVARCHAR(50) PRIMARY KEY, " +
                                                  "PopReceipt NVARCHAR(50) NULL, " +
                                                  "PopCount INTEGER, " +
                                                  "Popped NVARCHAR(25) NULL, " +
                                                  "Content NVARCHAR(2048) NULL)";
                createQueueTableCmd.ExecuteNonQuery();
            }
            using (SqliteCommand createPoisonTableCmd = new SqliteCommand()) {
                createPoisonTableCmd.Connection = _queueDatabase;
                createPoisonTableCmd.CommandText = "CREATE TABLE IF NOT EXISTS Poison(" +
                                                   "ID INTEGER PRIMARY KEY, " +
                                                   "PopReceipt NVARCHAR(50) NULL, " +
                                                   "PopCount INTEGER, " +
                                                   "Popped NVARCHAR(25) NULL, " +
                                                   "Content NVARCHAR(2048) NULL)";
                createPoisonTableCmd.ExecuteNonQuery();
            }
        }

        public Task<QueuedConversionJob> Queue(ConversionJobData job)
        {
            System.Diagnostics.Debug.WriteLine($"Queueing job to {_queueDatabase.DataSource} (Enter)");
            var id = Guid.NewGuid().ToString();
            using (var addMsgCmd = new SqliteCommand())
            {
                addMsgCmd.Connection = _queueDatabase;
                addMsgCmd.CommandText = "INSERT INTO Queue(ID, PopCount, Content) VALUES($QueueId, 0, $content)";
                addMsgCmd.Parameters.AddWithValue("$content", JsonConvert.SerializeObject(job));
                addMsgCmd.Parameters.AddWithValue("$QueueId", id);
                addMsgCmd.ExecuteScalar();
            }
            System.Diagnostics.Debug.WriteLine($"Queueing job to {_queueDatabase.DataSource} (Exit)");
            return Task.FromResult(new QueuedConversionJob
            {
                Id = id,
                PopReceipt = null,
                Content = job
            });
        }

        public Task<QueuedConversionJob> GetJob()
        {
            System.Diagnostics.Debug.WriteLine($"get message from {_queueDatabase.DataSource} (Enter)");

            CheckExpiredMessages();
            QueuedConversionJob queued = null;
            long popCount1;
            // now we can look for an entry to process.
            using (var getMsgCmd = new SqliteCommand())
            {
                getMsgCmd.Connection = _queueDatabase;
                getMsgCmd.CommandText = "SELECT ID, PopCount, Content from Queue WHERE PopReceipt IS NULL LIMIT 1";
                var reader = getMsgCmd.ExecuteReader();
                var popReceipt = Guid.NewGuid().ToString();
                var pop = reader["PopCount"];
                if (DBNull.Value.Equals(pop))
                {
                    popCount1 = 0;
                }
                else
                {
                    popCount1 = (long)pop;
                }
                popCount1++;

                try
                {
                    queued = new QueuedConversionJob
                    {
                        Id = reader["ID"].ToString(),
                        PopReceipt = popReceipt,
                        Content = JsonConvert.DeserializeObject<ConversionJobData>(reader["Content"].ToString())
                    };
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception thrown while getting new queued job - {ex}");
                    return null;
                }
            }

            using (var updateMsgCmd = new SqliteCommand())
            {
                updateMsgCmd.Connection = _queueDatabase;
                updateMsgCmd.CommandText =
                    "UPDATE Queue SET PopReceipt = $popReceipt, PopCount = $popCount, Popped = datetime('now') WHERE ID=$ID";
                updateMsgCmd.Parameters.AddWithValue("$ID", queued.Id);
                updateMsgCmd.Parameters.AddWithValue("$popReceipt", queued.PopReceipt);
                updateMsgCmd.Parameters.AddWithValue("$popCount", popCount1);
                updateMsgCmd.ExecuteReader();
            }
            System.Diagnostics.Debug.WriteLine($"get message from {_queueDatabase.DataSource} (Exit)");
            return string.IsNullOrWhiteSpace(queued.Id)
                ? Task.FromResult<QueuedConversionJob>(null)
                : Task.FromResult(queued);
        }

        public Task DeleteJob(QueuedConversionJob job)
        {
            System.Diagnostics.Debug.WriteLine($"deleting message from queue {_queueDatabase.DataSource} (Enter)");
            using (var delMsgCmd = new SqliteCommand())
            {
                delMsgCmd.Connection = _queueDatabase;
                delMsgCmd.CommandText = "DELETE FROM Queue WHERE ID=$ID AND PopReceipt=$popReceipt";
                delMsgCmd.Parameters.AddWithValue("$ID", job.Id);
                delMsgCmd.Parameters.AddWithValue("$popReceipt", job.PopReceipt);
                delMsgCmd.ExecuteReader();
            }
            System.Diagnostics.Debug.WriteLine($"deleting message from queue {_queueDatabase.DataSource} (Exit)");
            return Task.CompletedTask;
        }

        public void Clear() // clear the queue of entries - not normally desirable but useful for testing
        {
            System.Diagnostics.Debug.WriteLine($"clearing queue for {_queueDatabase.DataSource} (Enter)");
            using (var delMsgCmd = new SqliteCommand()) {
                delMsgCmd.Connection = _queueDatabase;
                delMsgCmd.CommandText = "DELETE FROM Queue";
                delMsgCmd.ExecuteNonQuery();
            }
            System.Diagnostics.Debug.WriteLine($"clearing queue for {_queueDatabase.DataSource} (Exit)");
        }

        private void CheckExpiredMessages()
        {
            System.Diagnostics.Debug.WriteLine($"expiring messages in queue {_queueDatabase.DataSource} (Enter)");

            // make sure there are no stale messages.
            // If there are we will reset their state (but increment a counter)
            using (var checkCmd = new SqliteCommand()) {
                checkCmd.Connection = _queueDatabase;
                checkCmd.CommandText = "SELECT ID, PopCount from Queue where PopReceipt IS NOT NULL AND Popped < datetime('now', $timeLimit)";
                checkCmd.Parameters.AddWithValue("$timeLimit", $"-{_queueTimeLimit} minutes");
                var chkReader = checkCmd.ExecuteReader();
                while (chkReader.Read())
                {
                    var id = (long)chkReader["ID"];
                    var popCount = (long)chkReader["PopCount"];
                    if (popCount > _queueEntryTries)
                    {   // we exceeded the retry counter so move the entry into the poison table.
                        using (var poison1Cmd = new SqliteCommand()) {
                            poison1Cmd.Connection = _queueDatabase;
                            poison1Cmd.CommandText = "INSERT INTO Poison(PopReceipt, PopCount, Popped, Content) SELECT PopReceipt, PopCount, Popped, Content FROM Queue WHERE ID=$ID";
                            poison1Cmd.Parameters.AddWithValue("$ID", id);
                            poison1Cmd.ExecuteNonQuery();
                        }
                        using (var poison2Cmd = new SqliteCommand()) {
                            poison2Cmd.Connection = _queueDatabase;
                            poison2Cmd.CommandText = "DELETE FROM Queue WHERE ID=$ID";
                            poison2Cmd.Parameters.AddWithValue("$ID", id);
                            poison2Cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {   // we haven't run out of attempts yet so reset the state but leave counter as-is
                        using (var resetPopCmd = new SqliteCommand()) {
                            resetPopCmd.Connection = _queueDatabase;
                            resetPopCmd.CommandText = "UPDATE Queue SET PopReceipt = NULL, Popped = NULL WHERE ID=$ID";
                            resetPopCmd.Parameters.AddWithValue("$ID", id);
                            resetPopCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine($"expiring messages in queue {_queueDatabase.DataSource} (Exit)");
        }
    }
}