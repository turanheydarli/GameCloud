using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Players.Requests;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Users.Requests;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services
{
    public class PlayerService(IPlayerRepository playerRepository) : IPlayerService
    {
        public async Task<AuthResponse> AuthenticatePlayerAsync(PlayerLoginRequest request)
        {
            if (request.Provider != AuthProvider.Custom)
            {
                throw new NotImplementedException("Only Custom auth provider is supported for now.");
            }

            if (string.IsNullOrEmpty(request.PlayerId))
            {
                throw new ArgumentException("PlayerId must be provided for Custom provider.");
            }

            var existingPlayer = await playerRepository.GetByPlayerIdAsync(request.PlayerId, request.Provider);

            if (existingPlayer == null)
            {
                var newPlayer = new Player
                {
                    PlayerId = request.PlayerId,
                    AuthProvider = request.Provider,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                existingPlayer = await playerRepository.CreateAsync(newPlayer);
            }

            // var token = _authTokenService.GenerateToken(existingPlayer.UserId, existingPlayer.Username);
            // var expiresAt = _authTokenService.GetExpirationTime();

            return new AuthResponse(
                existingPlayer.UserId,
                existingPlayer.PlayerId,
                "username",
                "token",
                DateTime.UtcNow
            );
        }
    }
}