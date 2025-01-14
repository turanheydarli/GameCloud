using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.Players.Requests;

public record PlayerRequest(
    Guid Id,
    AuthProvider AuthProvider,
    string? Username,
    Guid UserId);