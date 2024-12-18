using System.Text.Json;

namespace GameCloud.Domain.Entities;

public class ActionLog : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid PlayerId { get; set; }
    public string ActionType { get; set; }
    public JsonDocument Parameters { get; set; }
    public JsonDocument Result { get; set; }
    public Guid FunctionId { get; set; }
    public DateTime ExecutedAt { get; set; }
}