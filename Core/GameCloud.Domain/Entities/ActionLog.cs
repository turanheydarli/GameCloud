using System.Text.Json;
using GameCloud.Domain.Enums;

namespace GameCloud.Domain.Entities;

public class ActionLog : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string ActionType { get; set; }
    public bool IsTestMode { get; set; }
    public JsonDocument? Payload { get; set; }
    public JsonDocument? Result { get; set; }
    public Guid FunctionId { get; set; }
    public DateTime ExecutedAt { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public double ExecutionTimeMs { get; set; }
    public double TotalLatencyMs { get; set; }
    public FunctionStatus Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public int PayloadSizeBytes { get; set; }
    public int ResultSizeBytes { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public FunctionConfig? Function { get; set; }

    public bool IsSuccess()
    {
        return Status != FunctionStatus.Failed;
    }
}