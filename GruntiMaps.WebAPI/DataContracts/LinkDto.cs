using System;
namespace GruntiMaps.WebAPI.DataContracts
{
    public class LinkDto
    {
        public LinkDto(string rel, string href){
            Rel = rel;
            Href = href;
        }

        public string Rel { get; set; }
        public string Href { get; set; }
    }
}
