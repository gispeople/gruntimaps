namespace GruntiMaps.ResourceAccess.TopicSubscription
{
    public class MapLayerUpdateData
    {
        public string WorkspaceId { get; set; }
        public string MapLayerId { get; set; }
        public MapLayerUpdateType Type { get; set; }
    }

    public enum MapLayerUpdateType
    {
        Update,
        Delete
    }
}
