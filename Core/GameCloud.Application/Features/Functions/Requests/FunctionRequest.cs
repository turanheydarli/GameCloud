namespace GameCloud.Application.Features.Functions.Requests;

public record FunctionRequest(Guid GameId, string Name, string ActionType, string Endpoint, bool IsEnabled);