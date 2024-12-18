using System.Text.Json.Serialization;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.Players.Requests;

public record PlayerRegisterRequest(
    [property: JsonPropertyName("provider")] AuthProvider Provider,
    [property: JsonPropertyName("provider_token")] string Token,
    [property: JsonPropertyName("username")] string? Username,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("game_id")] Guid GameId 
);