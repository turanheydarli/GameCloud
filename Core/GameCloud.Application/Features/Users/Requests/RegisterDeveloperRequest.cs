namespace GameCloud.Application.Features.Users.Requests;

public record RegisterDeveloperRequest(
    string Name,
    string Email,
    string Password);

public record LoginDeveloperRequest(
    string Email,
    string Password);