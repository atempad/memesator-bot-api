using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace App.Models.DB;

[JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class Subscription : IEntity
{
    [JsonProperty(PropertyName = "id", Required = Required.Always)]
    public required string Id { get; set; }
  
    [JsonProperty(PropertyName = "subscriber_user_id", Required = Required.Always)]
    public required string SubscriberUserId { get; set; }
    
    [JsonProperty(PropertyName = "broadcaster_user_id", Required = Required.Always)]
    public required string BroadcasterUserId { get; set; }
}