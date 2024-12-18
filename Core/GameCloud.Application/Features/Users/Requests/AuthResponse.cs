namespace GameCloud.Application.Features.Users.Requests;

public record AuthResponse(
    Guid UserId,
    string PlayerId,
    string Email,
    string Token,
    DateTime ExpiresAt
);