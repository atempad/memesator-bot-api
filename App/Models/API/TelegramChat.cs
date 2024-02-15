using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace App.Models.API;

[JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class TelegramChat
{
    [JsonProperty(Required = Required.Always)]
    public long Id { get; set; }
}
