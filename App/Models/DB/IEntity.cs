using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace App.Models.DB;

[JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public interface IEntity
{
    [JsonProperty(PropertyName = "id", Required = Required.Always)]
    string Id { get; set; }
}