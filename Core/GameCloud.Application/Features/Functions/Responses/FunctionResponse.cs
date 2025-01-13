namespace GameCloud.Application.Features.Functions.Responses;

public record FunctionResponse(
    Guid Id,
    Guid GameId,
    string Name,
    string Description,
    string ActionType,
    string Endpoint,
    bool IsEnabled,
    DateTime CreatedAt,
    TimeSpan Timeout,
    Dictionary<string, string>? Headers,
    int MaxRetries
);