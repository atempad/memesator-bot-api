using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace App.Models.DB;

[JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class BotUser : IEntity
{
    [JsonProperty(Required = Required.Always)]
    public required string Id { get; set; }
    
    [JsonProperty(Required = Required.Always)]
    public required string ChatId { get; set; }
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Username { get; set; }

    public bool IsValid => !string.IsNullOrWhiteSpace(Id) && !string.IsNullOrWhiteSpace(ChatId);
}