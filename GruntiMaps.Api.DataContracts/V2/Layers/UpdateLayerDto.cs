using System.ComponentModel.DataAnnotations;

namespace GruntiMaps.Api.DataContracts.V2.Layers
{
    public class UpdateLayerDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string DataLocation { get; set; }
    }
}
