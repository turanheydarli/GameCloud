using GameCloud.Domain.Enums;
using System.Text.Json;

namespace GameCloud.Application.Features.Players.Responses;

public record PlayerResponse(
    Guid Id,
    string Username,
    string DisplayName,
    string? DeviceId,
    string? CustomId,
    PlayerStatus Status,
    Guid GameId,
    JsonDocument? Metadata,
    DateTime CreatedAt,
    DateTime? LastLoginAt);