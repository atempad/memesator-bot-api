using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Telegram.Bot.Types;

namespace Models;

[JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Subscriber
{
    [JsonProperty(Required = Required.Always)]
    public required string Id { get; set; }
    
    [JsonProperty(Required = Required.Always)]
    public required long ChatId { get; set; }
    
    [JsonProperty(Required = Required.Always)]
    public required DateTime SubscriptionDate { get; set; }
    
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Username { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? FirstName { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? LastName { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public ChatLocation? Location { get; set; }
}