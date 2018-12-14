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
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace GruntiMaps.ResourceAccess.Azure
{
    public class AzureConversionQueue : IConversionQueue
    {
        private readonly CloudQueue _queue;

        public AzureConversionQueue(string connectionString, string queueName)
        {
            _queue = CloudStorageAccount
                .Parse(connectionString)
                .CreateCloudQueueClient()
                .GetQueueReference(queueName);
            _queue.CreateIfNotExistsAsync();
        }

        public async Task<QueuedConversionJob> Queue(ConversionJobData job)
        {
            var queued = new CloudQueueMessage(JsonConvert.SerializeObject(job));
            await _queue.AddMessageAsync(queued);
            return new QueuedConversionJob
            {
                Id = queued.Id,
                PopReceipt = queued.PopReceipt,
                Content = JsonConvert.DeserializeObject<ConversionJobData>(queued.AsString), // todo: verify if this asString works as expected
            };
        }

        public async Task<QueuedConversionJob> GetJob()
        {
            // if there is a job on the queue, retrieve it.
            CloudQueueMessage queued = await _queue.GetMessageAsync();

            return queued != null
                ? new QueuedConversionJob
                {
                    Id = queued.Id,
                    Content = JsonConvert.DeserializeObject<ConversionJobData>(queued.AsString),
                    PopReceipt = queued.PopReceipt,
                }
                : null;
        }

        public async Task DeleteJob(QueuedConversionJob job)
        {
            await _queue.DeleteMessageAsync(job.Id, job.PopReceipt);
        }
    }
}
