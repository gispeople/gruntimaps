using System.ComponentModel.DataAnnotations;

namespace GruntiMaps.WebAPI.DataContracts
{
    public class CreateLayerDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string DataLocation { get; set; }
    }
}
