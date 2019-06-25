using System.Threading.Tasks;

namespace GruntiMaps.ResourceAccess.TopicSubscription
{
    public interface IMapLayerUpdateTopicClient
    {
        Task SendMessage(MapLayerUpdateData message);
    }
}
