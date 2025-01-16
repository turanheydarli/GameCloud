using System.Text.Json;
using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.Actions.Responses;

public record ActionResponse(
    Guid Id,
    Guid SessionId = default,
    Guid PlayerId = default,
    Guid FunctionId = default,
    string ActionType = default,
    DateTime CreatedAt = default,
    DateTime StartedAt = default,
    DateTime CompletedAt = default,
    double ExecutionTimeMs = 0,
    double TotalLatencyMs = 0,
    FunctionStatus Status = FunctionStatus.Pending,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    int RetryCount = 0,
    JsonDocument? Payload = null,
    JsonDocument? Result = null,
    int PayloadSizeBytes = 0,
    int ResultSizeBytes = 0,
    IDictionary<string, string>? Metadata = null
);

public record ActionStatsResponse(
    Guid FunctionId,
    string ActionType,
    int TotalExecutions,
    int SuccessfulExecutions,
    int FailedExecutions,
    double SuccessRate,
    double AverageExecutionTimeMs,
    double AverageLatencyMs,
    double P95LatencyMs,
    double P99LatencyMs,
    long TotalPayloadBytes,
    long TotalResultBytes,
    double AveragePayloadSizeBytes,
    double AverageResultSizeBytes,
    DateTime? LastExecutedAt,
    string? LastErrorCode,
    string? LastErrorMessage,
    IReadOnlyList<ErrorStat> TopErrors,
    DateTime WindowStart,
    DateTime WindowEnd
);

public record ErrorStat(
    string Code,
    string Message,
    int Count,
    DateTime LastOccurred,
    double PercentageOfErrors
);

public record ActionListStatsResponse(
    Guid GameId,
    int TotalFunctions,
    int ActiveFunctions,
    int TotalExecutions,
    double OverallSuccessRate,
    double AverageExecutionTimeMs,
    double AverageLatencyMs,
    double TotalInboundTrafficMB,
    double TotalOutboundTrafficMB,
    IReadOnlyList<ActionStatsResponse> FunctionStats,
    DateTime WindowStart,
    DateTime WindowEnd
);

public record DateTimeRange(DateTime From, DateTime To)
{
    public static DateTimeRange Last24Hours =>
        new(DateTime.UtcNow.AddHours(-24), DateTime.UtcNow);

    public static DateTimeRange LastWeek =>
        new(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

    public static DateTimeRange LastMonth =>
        new(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);

    public static DateTimeRange Today =>
        new(DateTime.UtcNow.Date, DateTime.UtcNow);
}