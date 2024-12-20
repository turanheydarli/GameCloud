namespace GameCloud.Application.Features.Games.Responses;

public record GameResponse(
    Guid Id,
    string Name,
    string Description,
    Guid DeveloperId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);