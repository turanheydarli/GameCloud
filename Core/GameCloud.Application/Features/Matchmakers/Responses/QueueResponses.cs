using System.Text.Json;

namespace GameCloud.Application.Features.Matchmakers.Responses;

public record QueueToggleResponse(bool Success, Guid QueueId, bool IsEnabled);

public record QueueStatsResponse
{
    public Guid QueueId { get; set; }
    public int ActiveMatches { get; set; }
    public int WaitingPlayers { get; set; }
    public double AvgWaitTimeSeconds { get; set; }
    public int TotalPlayers { get; set; }
    public double AverageWaitTime { get; set; }
    public string AverageWaitTimeTrend { get; set; }
    public double AverageWaitTimeChange { get; set; }
    public double MatchmakingSuccessRate { get; set; }
}

public record MatchmakingStatsResponse
{
    // public int ActiveQueues { get; set; }
    // public int ActiveMatches { get; set; }
    // public double AvgMatchmakingTime { get; set; }
    // public double MatchmakingSuccessRate { get; set; }
    public List<QueueStatsResponse> QueueStats { get; set; } = new();
}

public record QueueActivityResponse
{
    public List<string> Labels { get; set; } = new();
    public List<int> Matches { get; set; } = new();
    public List<int> Players { get; set; } = new();
}

public record QueueFunctionInfo(
    Guid Id,
    string Name,
    string ActionType
);

public record QueueFunctionsResponse
{
    public QueueFunctionInfo? Initialize { get; set; }
    public QueueFunctionInfo? Transition { get; set; }
    public QueueFunctionInfo? Leave { get; set; }
    public QueueFunctionInfo? End { get; set; }
}

public record QueueDashboardResponse
{
    public MatchmakingResponse Queue { get; set; }
    public QueueStatsResponse Stats { get; set; }
    public QueueFunctionsResponse Functions { get; set; }
}

public record MatchmakingLogEntry(
    Guid Id,
    DateTime Timestamp,
    string EventType,
    string QueueName,
    Guid? MatchId,
    Guid? PlayerId,
    JsonDocument Details
);