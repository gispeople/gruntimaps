using System.Threading.Tasks;
using GruntiMaps.ResourceAccess.Queue;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;

namespace GruntiMaps.ResourceAccess.Azure
{
    public class AzureQueue : IQueue
    {
        public CloudStorageAccount CloudAccount { get; }
        private CloudQueue QueueRef { get; }

        public AzureQueue(string key, string account, string queueName)
        {
            CloudAccount = new CloudStorageAccount(new StorageCredentials(account, key), true);
            var queueClient = CloudAccount.CreateCloudQueueClient();
            QueueRef = queueClient.GetQueueReference(queueName);
            QueueRef.CreateIfNotExistsAsync();
        }

        public async Task<string> AddMessage(string messageData)
        {
            CloudQueueMessage message = new CloudQueueMessage(messageData);
            await QueueRef.AddMessageAsync(message);
            return message.Id;
        }

        public async Task<Message> GetMessage()
        {
            // if there is a job on the queue, process it.
            var msg = await QueueRef.GetMessageAsync();
            if (msg == null)
            {
                return null;
            }

            return new Message
            {
                Id = msg.Id,
                Content = msg.AsString,
                PopReceipt = msg.PopReceipt,
            };
        }

        public async Task DeleteMessage(Message message)
        {
            await QueueRef.DeleteMessageAsync(message.Id, message.PopReceipt);
        }
    }
}
