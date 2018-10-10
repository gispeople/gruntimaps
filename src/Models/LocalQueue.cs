using System.Data.SqlTypes;
using System.Threading.Tasks;
using GruntiMaps.Interfaces;
using Microsoft.Data.Sqlite;

namespace GruntiMaps.Models
{
    public class LocalQueue: IQueue
    {
        private readonly SqliteConnection _queueDatabase;
        public LocalQueue(Options options, string queueName)
        {
            var builder = new SqliteConnectionStringBuilder
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Private,
                DataSource = System.IO.Path.Combine(options.StoragePath, $"{queueName}.queue")
            };
            var connStr = builder.ConnectionString;
            _queueDatabase = new SqliteConnection(connStr);
            _queueDatabase.Open();
            string createTable =
                "CREATE TABLE IF NOT EXISTS Queue(ID INTEGER PRIMARY KEY, PopReceipt NVARCHAR(50) NULL, Content NVARCHAR(2048) NULL)";
            SqliteCommand createTableCmd = new SqliteCommand(createTable, _queueDatabase);
            createTableCmd.ExecuteReader();
        }

        public async Task AddMessage(string message)
        {
            const string addMsg = "INSERT INTO Queue(Content) VALUES($content)";
            var addMsgCmd = new SqliteCommand(addMsg, _queueDatabase);
            addMsgCmd.Parameters.AddWithValue("$content", message);
            await addMsgCmd.ExecuteReaderAsync();
        }

        public async Task<Message> GetMessage()
        {
            const string getMsg = "SELECT ID, Content from Queue WHERE PopReceipt IS NULL LIMIT 1";
            var getMsgCmd = new SqliteCommand(getMsg, _queueDatabase);
            var reader = await getMsgCmd.ExecuteReaderAsync();
            var popReceipt = new SqlGuid().ToString();
            var msg = new Message
            {
                Id = reader["ID"].ToString(), 
                PopReceipt = popReceipt,
                Content = reader["Content"].ToString()
            };
            const string updateMsg = "UPDATE Queue SET PopReceipt = $popReceipt WHERE ID=$ID";
            var updateMsgCmd = new SqliteCommand(updateMsg, _queueDatabase);
            updateMsgCmd.Parameters.AddWithValue("$ID", msg.Id);
            updateMsgCmd.Parameters.AddWithValue("$popReceipt", msg.PopReceipt);
            await updateMsgCmd.ExecuteReaderAsync();
            if (string.IsNullOrWhiteSpace(msg.Id)) return null; // if there was no result send back an empty message - conforms with Azure's approach
            else return msg;
        }

        public async Task DeleteMessage(Message message)
        {
            const string delMsg = "DELETE FROM Queue WHERE ID=$ID AND PopReceipt=$popReceipt";
            var delMsgCmd = new SqliteCommand(delMsg, _queueDatabase);
            await delMsgCmd.ExecuteReaderAsync();
        }
    }
}