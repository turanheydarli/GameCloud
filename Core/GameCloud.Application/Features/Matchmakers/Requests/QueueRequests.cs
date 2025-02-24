using System.Text.Json;

namespace GameCloud.Application.Features.Matchmakers.Requests;

public record UpdateQueueFunctionsRequest(
    Guid? InitializeFunctionId,
    Guid? TransitionFunctionId,
    Guid? LeaveFunctionId,
    Guid? EndFunctionId
);

public record QueueTestRequest(
    int Players,
    JsonDocument Properties
);

public record QueueTestResponse(
    bool Success,
    Guid MatchId,
    List<TestPlayer> Players,
    double Duration
);

public record TestPlayer(
    Guid Id,
    JsonDocument Properties
);