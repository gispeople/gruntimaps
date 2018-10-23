using System.ComponentModel.DataAnnotations;

namespace GruntiMaps.WebAPI.Models
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
