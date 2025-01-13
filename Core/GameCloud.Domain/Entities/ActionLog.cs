using System.Text.Json;

namespace GameCloud.Domain.Entities;

public class ActionLog : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string ActionType { get; set; }
    public JsonDocument? Payload { get; set; }
    public JsonDocument? Result { get; set; }
    public Guid FunctionId { get; set; }
    public DateTime ExecutedAt { get; set; }
}