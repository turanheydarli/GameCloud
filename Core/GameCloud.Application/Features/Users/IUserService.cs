using GameCloud.Application.Features.Developers.Requests;
using GameCloud.Application.Features.Developers.Responses;
using GameCloud.Application.Features.Users.Requests;

namespace GameCloud.Application.Features.Users;

public interface IUserService
{
    Task<DeveloperResponse> RegisterDeveloperAsync(RegisterDeveloperRequest request);
    Task<AuthResponse> LoginDeveloperAsync(LoginDeveloperRequest request);
}