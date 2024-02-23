using Newtonsoft.Json;

namespace App.Models.Services.Youtube;

public class VideoStreamingFormat
{
    [JsonProperty("url")]
    public required string Url { get; set; }
    
    [JsonProperty("contentLength")]
    public required string ContentLength { get; set; }
}