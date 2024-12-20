namespace GameCloud.Application.Features.Functions.Responses;

public record FunctionResponse(Guid GameId, string Name, string ActionType, string Endpoint, bool IsEnabled);