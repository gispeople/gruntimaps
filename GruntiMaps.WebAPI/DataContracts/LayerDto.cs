using System;
using System.Collections.Generic;
using GruntiMaps.WebAPI.Models;
namespace GruntiMaps.WebAPI.DataContracts
{
    public class LayerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public LayerStatus Status { get; set; }
        public IList<LinkDto> Links { get; set; }
    }
}
