using System;
using System.Threading.Tasks;
using GruntiMaps.ResourceAccess.Queue;
using Microsoft.Data.Sqlite;

namespace GruntiMaps.ResourceAccess.Local
{
    public class LocalQueue : IQueue
    {
        private readonly SqliteConnection _queueDatabase;
        private readonly int _queueTimeLimit;
        private readonly int _queueEntryTries;

        public LocalQueue(string storagePath, int queueTimeLimit, int queueEntryTries, string queueName)
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
            const string createQueueTable = "CREATE TABLE IF NOT EXISTS Queue(ID NVARCHAR(50) PRIMARY KEY, PopReceipt NVARCHAR(50) NULL, PopCount INTEGER, Popped NVARCHAR(25) NULL, Content NVARCHAR(2048) NULL)";
            SqliteCommand createQueueTableCmd = new SqliteCommand(createQueueTable, _queueDatabase);
            createQueueTableCmd.ExecuteNonQuery();
            const string createPoisonTable = "CREATE TABLE IF NOT EXISTS Poison(ID INTEGER PRIMARY KEY, PopReceipt NVARCHAR(50) NULL, PopCount INTEGER, Popped NVARCHAR(25) NULL, Content NVARCHAR(2048) NULL)";
            SqliteCommand createPoisonTableCmd = new SqliteCommand(createPoisonTable, _queueDatabase);
            createPoisonTableCmd.ExecuteNonQuery();
        }

        public void Clear() // clear the queue of entries - not normally desirable but useful for testing
        {
            System.Diagnostics.Debug.WriteLine($"clearing queue for {_queueDatabase.DataSource} (Enter)");
            const string delMsg = "DELETE FROM Queue";
            var delMsgCmd = new SqliteCommand(delMsg, _queueDatabase);
            delMsgCmd.ExecuteNonQuery();
            System.Diagnostics.Debug.WriteLine($"clearing queue for {_queueDatabase.DataSource} (Exit)");
        }

        public Task<string> AddMessage(string message)
        {
            System.Diagnostics.Debug.WriteLine($"adding message to {_queueDatabase.DataSource} (Enter)");
            var id = Guid.NewGuid().ToString();
            const string addMsg = "INSERT INTO Queue(ID, PopCount, Content) VALUES($QueueId, 0, $content)";
            var addMsgCmd = new SqliteCommand(addMsg, _queueDatabase);
            addMsgCmd.Parameters.AddWithValue("$content", message);
            addMsgCmd.Parameters.AddWithValue("$QueueId", id);
            addMsgCmd.ExecuteScalar();
            System.Diagnostics.Debug.WriteLine($"adding message to {_queueDatabase.DataSource} (Exit)");
            return Task.FromResult(id);
        }

        public Task<Message> GetMessage()
        {
            System.Diagnostics.Debug.WriteLine($"get message from {_queueDatabase.DataSource} (Enter)");

            CheckExpiredMessages();
            // now we can look for an entry to process.
            const string getMsg = "SELECT ID, PopCount, Content from Queue WHERE PopReceipt IS NULL LIMIT 1";
            var getMsgCmd = new SqliteCommand(getMsg, _queueDatabase);
            var reader = getMsgCmd.ExecuteReader();
            var popReceipt = Guid.NewGuid().ToString();
            var pop = reader["PopCount"];
            long popCount1;
            if (DBNull.Value.Equals(pop))
            {
                popCount1 = 0;
            }
            else
            {
                popCount1 = (long)pop;
            }
            popCount1++;
            var msg = new Message
            {
                Id = reader["ID"].ToString(),
                PopReceipt = popReceipt,
                Content = reader["Content"].ToString()
            };
            const string updateMsg = "UPDATE Queue SET PopReceipt = $popReceipt, PopCount = $popCount, Popped = datetime('now') WHERE ID=$ID";
            var updateMsgCmd = new SqliteCommand(updateMsg, _queueDatabase);
            updateMsgCmd.Parameters.AddWithValue("$ID", msg.Id);
            updateMsgCmd.Parameters.AddWithValue("$popReceipt", msg.PopReceipt);
            updateMsgCmd.Parameters.AddWithValue("$popCount", popCount1);
            updateMsgCmd.ExecuteReader();
            System.Diagnostics.Debug.WriteLine($"get message from {_queueDatabase.DataSource} (Exit)");
            return string.IsNullOrWhiteSpace(msg.Id) ? Task.FromResult<Message>(null) : Task.FromResult(msg);
        }

        private void CheckExpiredMessages()
        {
            System.Diagnostics.Debug.WriteLine($"expiring messages in queue {_queueDatabase.DataSource} (Enter)");

            // make sure there are no stale messages.
            // If there are we will reset their state (but increment a counter)
            const string check =
                "SELECT ID, PopCount from Queue where PopReceipt IS NOT NULL AND Popped < datetime('now', $timeLimit)";
            var checkCmd = new SqliteCommand(check, _queueDatabase);
            checkCmd.Parameters.AddWithValue("$timeLimit", $"-{_queueTimeLimit} minutes");
            var chkReader = checkCmd.ExecuteReader();
            while (chkReader.Read())
            {
                var id = (long)chkReader["ID"];
                var popCount = (long)chkReader["PopCount"];
                if (popCount > _queueEntryTries)
                {   // we exceeded the retry counter so move the entry into the poison table.
                    const string poison1 = "INSERT INTO Poison(PopReceipt, PopCount, Popped, Content) SELECT PopReceipt, PopCount, Popped, Content FROM Queue WHERE ID=$ID";
                    var poison1Cmd = new SqliteCommand(poison1, _queueDatabase);
                    poison1Cmd.Parameters.AddWithValue("$ID", id);
                    poison1Cmd.ExecuteNonQuery();
                    const string poison2 = "DELETE FROM Queue WHERE ID=$ID";
                    var poison2Cmd = new SqliteCommand(poison2, _queueDatabase);
                    poison2Cmd.Parameters.AddWithValue("$ID", id);
                    poison2Cmd.ExecuteNonQuery();
                }
                else
                {   // we haven't run out of attempts yet so reset the state but leave counter as-is
                    const string resetPop = "UPDATE Queue SET PopReceipt = NULL, Popped = NULL WHERE ID=$ID";
                    var resetPopCmd = new SqliteCommand(resetPop, _queueDatabase);
                    resetPopCmd.Parameters.AddWithValue("$ID", id);
                    resetPopCmd.ExecuteNonQuery();
                }
            }
            System.Diagnostics.Debug.WriteLine($"expiring messages in queue {_queueDatabase.DataSource} (Exit)");

        }

        public Task DeleteMessage(Message message)
        {
            System.Diagnostics.Debug.WriteLine($"deleting message from queue {_queueDatabase.DataSource} (Enter)");
            const string delMsg = "DELETE FROM Queue WHERE ID=$ID AND PopReceipt=$popReceipt";
            var delMsgCmd = new SqliteCommand(delMsg, _queueDatabase);
            delMsgCmd.Parameters.AddWithValue("$ID", message.Id);
            delMsgCmd.Parameters.AddWithValue("$popReceipt", message.PopReceipt);
            delMsgCmd.ExecuteReader();
            System.Diagnostics.Debug.WriteLine($"deleting message from queue {_queueDatabase.DataSource} (Exit)");
            return Task.CompletedTask;
        }
    }
}