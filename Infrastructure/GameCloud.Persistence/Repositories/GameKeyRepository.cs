using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class GameKeyRepository(GameCloudDbContext context) : IGameKeyRepository
{
    private DbSet<GameKey> GameKeys => context.Set<GameKey>();

    public async Task<GameKey> GetByApiKeyAsync(string gameKey)
    {
        if (string.IsNullOrWhiteSpace(gameKey))
            throw new ArgumentException("Game key cannot be null or empty.", nameof(gameKey));

        return await GameKeys.FirstOrDefaultAsync(gk => gk.ApiKey == gameKey);
    }

    public async Task RevokeAsync(GameKey gameKey)
    {
        gameKey.Status = GameKeyStatus.Revoked;
        context.Entry(gameKey).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }
}