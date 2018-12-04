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
