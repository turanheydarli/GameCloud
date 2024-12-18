using System.Text.Json;

namespace GameCloud.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid SubscriptionId { get; set; }
        
    public Guid PlayerId { get; set; } 
    public Guid? SessionId { get; set; }    

    public string? Title { get; set; }
    public string? Body { get; set; }
    public string? Icon { get; set; }
    public JsonDocument? Data { get; set; }

    public Guid? ActionId { get; set; }          
    public DateTime SentAt { get; set; }
    public NotificationStatus Status { get; set; }
}