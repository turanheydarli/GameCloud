using System.Text.Json.Serialization;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.Players.Requests;

public record PlayerLoginRequest(
    AuthProvider Provider,
    string Token,
    string? Username,
    string? PlayerId,
    Guid UserId
);