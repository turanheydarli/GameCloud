using System.Text.Json.Serialization;

namespace GameCloud.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FunctionStatus
{
    Success,
    Failed,
    PartialSuccess,
    Pending
}