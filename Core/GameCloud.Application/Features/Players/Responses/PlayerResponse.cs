using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.Players.Responses;

public record PlayerResponse(
    Guid Id,
    AuthProvider AuthProvider,
    PlayerStatus Status,
    string? PlayerId,
    Guid UserId);

public record AttributeResponse(
    string Key,
    object Value,
    string Type);