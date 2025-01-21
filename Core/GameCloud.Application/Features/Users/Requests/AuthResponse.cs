namespace GameCloud.Application.Features.Users.Requests;

public record AuthResponse(
    Guid UserId,
    string Username,
    string Email,
    string Token,
    DateTime ExpiresAt
);