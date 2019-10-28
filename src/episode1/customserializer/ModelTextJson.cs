using System.Text.Json.Serialization;

namespace episode1
{
    public class ModelTextJson
    {
        [JsonPropertyName("id")]
        public string TheIdentifier { get; set; }

        [JsonPropertyName("title")]
        public string DescriptiveTitle { get; set;}
    }
}