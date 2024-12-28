using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameCloud.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationChannel
{
    InApp,
    Push,
    WebSocket,
    Email,
}

public class Notification : BaseEntity
{
    public Guid From { get; set; }
    public Guid To { get; set; }

    public Guid? SubscriptionId { get; set; }
    public Guid? SessionId { get; set; }

    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Icon { get; set; }

    public JsonDocument? Data { get; set; }

    public Guid? ActionId { get; set; }

    public NotificationChannel Channel { get; set; }

    public DateTime SentAt { get; set; }
    public NotificationStatus Status { get; set; }
}