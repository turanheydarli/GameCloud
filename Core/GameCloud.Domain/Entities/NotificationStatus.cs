using System.Text.Json.Serialization;

namespace GameCloud.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationStatus
{
    Pending,
    Sent,
    Delivered,
    Read,
    Failed
}