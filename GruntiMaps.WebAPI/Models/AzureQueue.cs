using System.Threading.Tasks;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace GruntiMaps.WebAPI.Models
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
            QueueRef = queueClient.GetQueueReference(queueName);
            QueueRef.CreateIfNotExistsAsync();
        }
        public async Task<string> AddMessage(string messageData)
        {

            var jsonMsg = JsonConvert.SerializeObject(messageData);
            CloudQueueMessage message = new CloudQueueMessage(jsonMsg);
            await QueueRef.AddMessageAsync(message);
            return message.Id;
        }

        public async Task<Message> GetMessage()
        {
            Message result = new Message();
            // if there is a job on the queue, process it.
            var msg = await QueueRef.GetMessageAsync();
            if (msg == null) return result;
            result.Id = msg.Id;
            result.Content = msg.AsString;
            result.PopReceipt = msg.PopReceipt;

            return result;
        }

        public async Task DeleteMessage(Message message)
        {
            await QueueRef.DeleteMessageAsync(message.Id, message.PopReceipt);
        }
    }
}