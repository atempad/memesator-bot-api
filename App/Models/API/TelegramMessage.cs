using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace App.Models.API;

[JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class TelegramMessage
{
    [JsonProperty(Required = Required.Always)]
    public int MessageId { get; set; }
    
    [JsonProperty(Required = Required.Always)]
    public TelegramChat Chat { get; set; } = default!;
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public TelegramUser? From { get; set; }
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Text { get; set; }
}