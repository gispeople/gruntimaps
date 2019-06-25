using System;
using System.Threading.Tasks;

namespace GruntiMaps.ResourceAccess.TopicSubscription
{
    public interface IMapLayerUpdateSubscriptionClient
    {
        void RegisterOnMessageHandlerAndReceiveMessages(Func<MapLayerUpdateData, Task> function);
    }
}
