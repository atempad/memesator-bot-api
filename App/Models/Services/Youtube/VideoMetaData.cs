using Newtonsoft.Json;

namespace App.Models.Services.Youtube;

public class VideoMetaData
{
    [JsonProperty("streamingData")]
    public required VideoStreamingData StreamingData { get; set; }
}
