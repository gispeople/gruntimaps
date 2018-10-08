using System.Threading.Tasks;
using GruntiMaps.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace GruntiMaps.Models
{
    public class AzureQueue: IQueue
    {
        public CloudStorageAccount CloudAccount { get; }

        public AzureQueue(Options options)
        {
            CloudAccount =
                new CloudStorageAccount(
                    new StorageCredentials(options.StorageAccount, options.StorageKey), true);
        }
        public async Task AddMessage(string queueName, string messageData)
        {
            var queueClient = CloudAccount.CreateCloudQueueClient();
            // _logger.LogDebug($"Monitoring {_mapdata.CurrentOptions.GdConvQueue}");
            var queueRef = queueClient.GetQueueReference(queueName);
            await queueRef.CreateIfNotExistsAsync();

            var jsonMsg = JsonConvert.SerializeObject(messageData);
            CloudQueueMessage message = new CloudQueueMessage(jsonMsg);
            await queueRef.AddMessageAsync(message);
        }

        public async Task<Message> GetMessage(string queueName)
        {
            Message result = new Message();
            var queueClient = CloudAccount.CreateCloudQueueClient();
            // _logger.LogDebug($"Monitoring {_mapdata.CurrentOptions.MbConvQueue}");
            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            // if there is a job on the queue, process it.
            var msg = await queue.GetMessageAsync();
            if (msg != null) // if no message, don't try
            {
                result.ID = msg.Id;
                result.Content = msg.AsString;
                result.PopReceipt = msg.PopReceipt;
            }

            return result;
        }

        public async Task DeleteMessage(string queueName, Message message)
        {
            var queueClient = CloudAccount.CreateCloudQueueClient();
            // _logger.LogDebug($"Monitoring {_mapdata.CurrentOptions.MbConvQueue}");
            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            await queue.DeleteMessageAsync(message.ID, message.PopReceipt);
        }
    }
}