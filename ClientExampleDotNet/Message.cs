using System.Text.Json.Serialization;

namespace ClientExampleDotNet
{

    public class Message
    {
        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        /// <summary>
        /// type for example: message
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("to")]
        public List<string> To { get; set; }

        [JsonPropertyName("payload")]
        public Payload Payload { get; set; }
    }

    public class Payload
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
