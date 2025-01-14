namespace GameCloud.Application.Features.Players.Requests;

public record AttributeRequest(
    string Key,
    string Value,
    string ExpectedVersion,
    int? ExpiresIn,
    Dictionary<string, object> PermissionRead,
    Dictionary<string, object> PermissionWrite);
