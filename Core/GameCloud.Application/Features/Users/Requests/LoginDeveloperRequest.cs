namespace GameCloud.Application.Features.Users.Requests;

public record LoginDeveloperRequest(
    string Email,
    string Password);