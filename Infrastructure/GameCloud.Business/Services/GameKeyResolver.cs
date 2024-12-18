using GameCloud.Application.Common.Interfaces;
using GameCloud.Application.Features.Games;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class GameKeyResolver(IGameKeyRepository gameKeyRepository) : IGameKeyResolver
{
    public async Task<Guid> ResolveGameIdAsync(string gameKey)
    {
        var gameKeyEntity = await gameKeyRepository.GetByApiKeyAsync(gameKey);
        
        if (gameKeyEntity == null || gameKeyEntity.Status != GameKeyStatus.Active)
            throw new UnauthorizedAccessException("Invalid or revoked game key.");

        return gameKeyEntity.GameId;
    }
}