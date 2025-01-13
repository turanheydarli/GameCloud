using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.Players.Requests;

public record PlayerRequest(
    Guid Id,
    AuthProvider AuthProvider,
    string? PlayerId,
    string? Username,
    Guid UserId);

public record AttributeRequest(
    string Key,
    object Value);
