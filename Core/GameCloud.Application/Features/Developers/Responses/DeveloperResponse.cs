namespace GameCloud.Application.Features.Developers.Responses;

public record DeveloperResponse(
    Guid Id,
    string Name,
    string Email,
    string? ProfileImageUrl=null);