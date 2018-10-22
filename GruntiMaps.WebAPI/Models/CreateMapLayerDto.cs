using System.ComponentModel.DataAnnotations;

namespace GruntiMaps.Models
{
    public class CreateMapLayerDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string DataLocation { get; set; }
    }
}
