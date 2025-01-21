using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.Users.Requests;

public record AuthPlayerRequest(
    AuthProvider Provider,
    string? Token,
    string UserName,
    Guid UserId);