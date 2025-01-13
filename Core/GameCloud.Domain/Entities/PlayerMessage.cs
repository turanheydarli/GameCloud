using System.Text.Json;

namespace GameCloud.Domain.Entities;

public enum MessageStatus
{
    Sent,
    Unread,
    Failed
}

public class PlayerMessage : BaseEntity
{
    public Guid UserId { get; set; } 

    public string? Type { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    
    public JsonDocument? Data { get; set; }
    public DateTime SentAt { get; set; }
    public MessageStatus Status { get; set; }
}