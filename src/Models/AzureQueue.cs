using System.Threading.Tasks;
using GruntiMaps.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace GruntiMaps.Models
{
    public class AzureQueue: IQueue
    {
        public CloudStorageAccount CloudAccount { get; }
        private CloudQueue QueueRef { get; }
        public AzureQueue(Options options, string queueName)
        {
            CloudAccount =
                new CloudStorageAccount(
                    new StorageCredentials(options.StorageAccount, options.StorageKey), true);
            var queueClient = CloudAccount.CreateCloudQueueClient();
            // _logger.LogDebug($"Monitoring {_mapdata.CurrentOptions.GdConvQueue}");
            QueueRef = queueClient.GetQueueReference(queueName);
            QueueRef.CreateIfNotExistsAsync();
        }
        public async Task AddMessage(string messageData)
        {

            var jsonMsg = JsonConvert.SerializeObject(messageData);
            CloudQueueMessage message = new CloudQueueMessage(jsonMsg);
            await QueueRef.AddMessageAsync(message);
        }

        public async Task<Message> GetMessage()
        {
            Message result = new Message();
            // if there is a job on the queue, process it.
            var msg = await QueueRef.GetMessageAsync();
            if (msg != null) // if no message, don't try
            {
                result.ID = msg.Id;
                result.Content = msg.AsString;
                result.PopReceipt = msg.PopReceipt;
            }

            return result;
        }

        public async Task DeleteMessage(Message message)
        {
            await QueueRef.DeleteMessageAsync(message.ID, message.PopReceipt);
        }
    }
}