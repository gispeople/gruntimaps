using System;
using System.Text;
using System.Threading.Tasks;
using GruntiMaps.ResourceAccess.TopicSubscription;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace GruntiMaps.ResourceAccess.Azure
{
    public class AzureMapLayerUpdateTopicClient : IMapLayerUpdateTopicClient, IDisposable
    {
        private readonly ITopicClient _client;

        public AzureMapLayerUpdateTopicClient(string connectionString, string topic)
        {
            _client = new TopicClient(connectionString ?? throw new ArgumentNullException(nameof(connectionString)), 
                topic ?? throw new ArgumentNullException(nameof(topic)));
        }

        public Task SendMessage(MapLayerUpdateData message)
        {
            var messageByte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            return _client.SendAsync(new Message(messageByte));
        }

        public void Dispose()
        {
            _client.CloseAsync();
        }
    }
}
