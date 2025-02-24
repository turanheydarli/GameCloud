using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.Players.Requests;

public record PlayerRequest(
    Guid Id,
    AuthProvider AuthProvider,
    string? Username,
    string? DisplayName,
    Guid UserId);