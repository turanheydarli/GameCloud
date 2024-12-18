using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Players.Requests;
using GameCloud.Application.Features.Users.Requests;

namespace GameCloud.Application.Features.Players;

public interface IPlayerService
{
    Task<AuthResponse> AuthenticatePlayerAsync(PlayerLoginRequest request);
}