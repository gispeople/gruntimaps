using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GruntiMaps.ResourceAccess.TopicSubscription;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GruntiMaps.ResourceAccess.Azure
{
    public class AzureMapLayerUpdateSubscriptionClient : IMapLayerUpdateSubscriptionClient
    {
        private readonly ISubscriptionClient _client;
        private readonly ILogger _logger;
        private bool _registered;

        public AzureMapLayerUpdateSubscriptionClient(string connectionString, 
            string topic, 
            string subscription,
            ILogger logger)
        {
            _logger = logger;
            _client = new SubscriptionClient(connectionString, topic, subscription);
        }

        public void RegisterOnMessageHandlerAndReceiveMessages(Func<MapLayerUpdateData, Task> function)
        {
            if (!_registered)
            {
                // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
                var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                    // Set it according to how many messages the application wants to process in parallel.
                    MaxConcurrentCalls = 1,

                    // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                    // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                    AutoComplete = false
                };

                // Register the function that processes messages.
                _client.RegisterMessageHandler(GetProcessFunction(function), messageHandlerOptions);

                _registered = true;
            }
        }

        private Func<Message, CancellationToken, Task> GetProcessFunction(Func<MapLayerUpdateData, Task> function)
        {
            return async (message, token) =>
            {
                _logger.LogDebug($"Processing Topic message {message.MessageId}");

                var mapLayerUpdateData =
                    JsonConvert.DeserializeObject<MapLayerUpdateData>(Encoding.UTF8.GetString(message.Body));

                _logger.LogDebug($"Topic message deserialized for layer {mapLayerUpdateData.WorkspaceId}/{mapLayerUpdateData.MapLayerId} for {mapLayerUpdateData.Type}");

                await function(mapLayerUpdateData);

                _logger.LogDebug($"Topic message processed for layer {mapLayerUpdateData.WorkspaceId}/{mapLayerUpdateData.MapLayerId} for {mapLayerUpdateData.Type}");

                // Complete the message so that it is not received again.
                // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
                await _client.CompleteAsync(message.SystemProperties.LockToken);
            };
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError($"Subscription Message handling failed for {exceptionReceivedEventArgs.ExceptionReceivedContext.Action}", exceptionReceivedEventArgs.Exception);
            return Task.CompletedTask;
        }
    }
}
