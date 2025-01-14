namespace GameCloud.Application.Features.Games.Responses;

public record GameDetailResponse(
    Guid Id,
    string Name,
    string Description,
    Guid DeveloperId,
    Guid? ImageId,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int TotalPlayerCount,
    int ActivePlayerCount,
    int FunctionCount,
    int KeyCount,
    IEnumerable<GameActivityResponse> RecentActivity
);