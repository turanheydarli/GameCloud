using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.Players.Requests;

public record PlayerRequest(
    Guid Id,
    string? Username,
    string? DisplayName,
    Dictionary<string, object>? Metadata = null);