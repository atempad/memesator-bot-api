using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace App.Models.API;

[JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class TelegramUpdate
{
    [JsonProperty("update_id", Required = Required.Always)]
    public int Id { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public TelegramMessage? Message { get; set; }
}
