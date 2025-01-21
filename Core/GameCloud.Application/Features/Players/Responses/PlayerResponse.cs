using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.Players.Responses;

public record PlayerResponse(
    Guid Id,
    AuthProvider AuthProvider,
    PlayerStatus Status,
    string? Username,
    Guid UserId);