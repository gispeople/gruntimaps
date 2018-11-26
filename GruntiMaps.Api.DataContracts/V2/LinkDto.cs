namespace GruntiMaps.Api.DataContracts.V2
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
