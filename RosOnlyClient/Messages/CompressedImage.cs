using System.Text.Json.Serialization;

namespace RosOnlyClient.Messages
{
    // For rosbridge, CompressedImage.data is base64 string
    public class CompressedImage
    {
        public string format { get; set; } = "jpeg";
        [JsonPropertyName("data")] public string data_base64 { get; set; } = "";
    }
}
