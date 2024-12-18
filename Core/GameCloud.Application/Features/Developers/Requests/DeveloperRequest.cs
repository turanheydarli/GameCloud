namespace GameCloud.Application.Features.Developers.Requests;

public record DeveloperRequest(Guid Id, Guid UserId, string Name, string Email);