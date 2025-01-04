namespace GameCloud.Application.Features.Games.Requests;

public record GameRequest(
    string Name,
    string Description,
    Guid DeveloperId,
    bool IsEnabled
);