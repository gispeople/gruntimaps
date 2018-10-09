using System.Data.SqlTypes;
using System.Threading.Tasks;
using GruntiMaps.Interfaces;
using Microsoft.Data.Sqlite;

namespace GruntiMaps.Models
{
    public class LocalQueue: IQueue
    {
        private string QueueName;
        private Options CurrentOptions;
        private SqliteConnection QueueDatabase;
        public LocalQueue(Options options, string queueName)
        {
            QueueName = queueName;
            CurrentOptions = options;
            var builder = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Private,
                DataSource = System.IO.Path.Combine(options.StoragePath, $"{QueueName}.queue")
            };
            var connStr = builder.ConnectionString;
            QueueDatabase = new SqliteConnection(connStr);
            QueueDatabase.Open();
            string createTable =
                "CREATE TABLE IF NOT EXISTS Queue(ID INTEGER PRIMARY KEY, PopReceipt NVARCHAR(50) NULL, Content NVARCHAR(2048) NULL)";
            SqliteCommand createTableCmd = new SqliteCommand(createTable, QueueDatabase);
            createTableCmd.ExecuteReader();
        }

        public async Task AddMessage(string message)
        {
            const string addMsg = "INSERT INTO Queue(Content) VALUES($content)";
            var addMsgCmd = new SqliteCommand(addMsg, QueueDatabase);
            addMsgCmd.Parameters.AddWithValue("$content", message);
            await addMsgCmd.ExecuteReaderAsync();
        }

        public async Task<Message> GetMessage()
        {
            const string getMsg = "SELECT ID, Content from Queue WHERE PopReceipt IS NULL LIMIT 1";
            var getMsgCmd = new SqliteCommand(getMsg, QueueDatabase);
            var reader = await getMsgCmd.ExecuteReaderAsync();
            var popReceipt = new SqlGuid().ToString();
            var msg = new Message
            {
                ID = reader["ID"].ToString(), 
                PopReceipt = popReceipt,
                Content = reader["Content"].ToString()
            };
            const string updateMsg = "UPDATE Queue SET PopReceipt = $popReceipt WHERE ID=$ID";
            var updateMsgCmd = new SqliteCommand(updateMsg, QueueDatabase);
            updateMsgCmd.Parameters.AddWithValue("$ID", msg.ID);
            updateMsgCmd.Parameters.AddWithValue("$popReceipt", msg.PopReceipt);
            await updateMsgCmd.ExecuteReaderAsync();
            if (string.IsNullOrWhiteSpace(msg.ID)) return null; // if there was no result send back an empty message - conforms with Azure's approach
            else return msg;
        }

        public async Task DeleteMessage(Message message)
        {
            const string delMsg = "DELETE FROM Queue WHERE ID=$ID AND PopReceipt=$popReceipt";
            var delMsgCmd = new SqliteCommand(delMsg, QueueDatabase);
            await delMsgCmd.ExecuteReaderAsync();
        }
    }
}