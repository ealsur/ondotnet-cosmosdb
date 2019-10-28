using System.Text.Json.Serialization;

namespace episode1
{
    public class Model
    {
        [JsonPropertyName("id")]
        public string TheIdentifier { get; set; }

        [JsonPropertyName("title")]
        public string DescriptiveTitle { get; set;}
    }
}