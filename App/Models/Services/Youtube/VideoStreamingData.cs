using Newtonsoft.Json;

namespace App.Models.Services.Youtube;

public class VideoStreamingData
{
    [JsonProperty("formats")]
    public required List<VideoStreamingFormat> Formats { get; set; }
}
