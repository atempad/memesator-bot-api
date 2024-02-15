using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace App.Models.DB;

[JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Subscription : IEntity
{
    [JsonProperty(Required = Required.Always)]
    public required string Id { get; set; }
  
    [JsonProperty(Required = Required.Always)]
    public required string SubscriberUserId { get; set; }
    
    [JsonProperty(Required = Required.Always)]
    public required string BroadcasterUserId { get; set; }
}