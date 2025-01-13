using System.Text.Json.Serialization;

namespace GameCloud.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlayerStatus
{
    Online,
    Offline,
    InGame
}