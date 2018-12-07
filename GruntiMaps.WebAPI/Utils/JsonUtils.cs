using System.IO;
using Newtonsoft.Json;

namespace GruntiMaps.WebAPI.Utils
{
    public static class JsonUtils
    {
        public static string JsonPrettify(string json)
        {
            if (json == null)
            {
                return "{}";
            }

            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }
    }
}
