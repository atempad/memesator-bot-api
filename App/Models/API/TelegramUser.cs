using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace App.Models.API;

[JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class TelegramUser
{
    [JsonProperty(Required = Required.Always)]
    public long Id { get; set; }
    
    [JsonProperty(Required = Required.Always)]
    public string FirstName { get; set; } = default!;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? LastName { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Username { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? LanguageCode { get; set; }
}