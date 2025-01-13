namespace GameCloud.Application.Features.Games.Responses;

public record GameResponse(
    Guid Id,
    string Name,
    string Description,
    Guid DeveloperId,
    Guid ImageId,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt
);