using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Games.Responses;

public record GameKeyResponse(
    Guid Id,
    Guid GameId,
    string ApiKey,
    GameKeyStatus Status
);