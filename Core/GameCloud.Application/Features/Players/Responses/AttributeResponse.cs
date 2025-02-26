using GameCloud.Domain.Entities;

namespace GameCloud.Application.Features.Players.Responses;

public record AttributeResponse(
    string Username,
    string Collection,
    string Key,
    string Value,
    string Version,
    Dictionary<string, object>? PermissionRead,
    Dictionary<string, object>? PermissionWrite);

public record AttributeCollectionResponse(Dictionary<string, AttributeResponse> Collections);