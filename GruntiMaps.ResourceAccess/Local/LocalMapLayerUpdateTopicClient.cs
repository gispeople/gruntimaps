using System;
using System.Text;
using System.Threading.Tasks;
using GruntiMaps.ResourceAccess.TopicSubscription;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace GruntiMaps.ResourceAccess.Local
{
    public class LocalMapLayerUpdateTopicClient : IMapLayerUpdateTopicClient
    {
        // private readonly ITopicClient _client;
        private readonly ILogger _logger;
        public LocalMapLayerUpdateTopicClient(ILogger logger)
        {
            _logger = logger ?? throw new NullReferenceException(nameof(logger));
            // _client = new TopicClient(connectionString ?? throw new ArgumentNullException(nameof(connectionString)), 
            //     topic ?? throw new ArgumentNullException(nameof(topic)));
        }

        public Task SendMessage(MapLayerUpdateData message)
        {
            _logger.LogInformation("SendMessage called");
            var messageByte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            return Task.FromResult<bool>(true);
            // return _client.SendAsync(new Message(messageByte));
        }
    }
}
