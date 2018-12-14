using GruntiMaps.Common.Enums;

namespace GruntiMaps.Api.DataContracts.V2.Layers
{
    public class LayerStatusDto
    {
        public string Id { get; set; }
        public LayerStatus Status { get; set; }
    }
}
