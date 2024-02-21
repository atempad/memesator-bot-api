using App.Services.Permissions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace App.Models.DB;

[JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ServiceUser : IEntity
{
    [JsonProperty(PropertyName = "id", Required = Required.Always)]
    public required string Id { get; set; }
    
    [JsonProperty(PropertyName = "chat_id", Required = Required.Always)]
    public required string ChatId { get; set; }
    
    [JsonProperty(PropertyName = "role", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public Role? Role { get; set; }
    
    [JsonProperty(PropertyName = "username", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string? Username { get; set; }
}