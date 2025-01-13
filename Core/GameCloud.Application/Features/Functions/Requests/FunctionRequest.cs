namespace GameCloud.Application.Features.Functions.Requests;

public record FunctionRequest(
    Guid GameId,
    string Name,
    string? Description,
    string ActionType,
    string Endpoint,
    bool IsEnabled,
    TimeSpan Timeout,
    Dictionary<string, string>? Headers,
    int MaxRetries
);