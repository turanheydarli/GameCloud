using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class PlayerRepository(GameCloudDbContext context) : IPlayerRepository
{
    public async Task<IPaginate<Player>> GetAllAsync(int index = 0, int size = 10, bool enableTracking = true)
    {
        IQueryable<Player> queryable = context.Set<Player>();
        if (!enableTracking)
            queryable.AsNoTracking();

        return await queryable.ToPaginateAsync(index, size, 0);
    }

    public async Task<Player> CreateAsync(Player developer)
    {
        context.Entry(developer).State = EntityState.Added;
        await context.SaveChangesAsync();
        return developer;
    }

    public async Task<Player?> GetByIdAsync(Guid id)
    {
        IQueryable<Player?> queryable = context.Set<Player>();

        queryable = queryable.Where(player => player != null && player.Id == id);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<Player?> GetByUserIdAsync(Guid userId)
    {
        IQueryable<Player?> queryable = context.Set<Player>();

        queryable = queryable.Where(player => player != null && player.UserId == userId);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<Player?> GetByUsernameAsync(string playerId, AuthProvider provider)
    {
        IQueryable<Player?> queryable = context.Set<Player>();

        queryable = queryable.Where(player =>
            player != null && player.Username == playerId && player.AuthProvider == provider);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<Player?> GetByUsernameAsync(string playerId)
    {
        IQueryable<Player?> queryable = context.Set<Player>();

        queryable = queryable.Where(player =>
            player != null && player.Username == playerId);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<Player> UpdateAsync(Player player)
    {
        context.Entry(player).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return player;
    }

    public async Task<Player> GetByDeviceIdAsync(Guid gameId, string deviceId)
    {
        IQueryable<Player?> queryable = context.Set<Player>();

        return await queryable.FirstOrDefaultAsync(p => p.DeviceId == deviceId && p.GameId == gameId);
    }

    public async Task<Player> GetByCustomIdAsync(Guid gameId, string customId)
    {
        IQueryable<Player?> queryable = context.Set<Player>();

        return await queryable.FirstOrDefaultAsync(p => p.CustomId == customId && p.GameId == gameId);
    }

    public async Task<IPaginate<Player>> GetAllByGameId(Guid gameId, int index, int size, bool enableTracking = true)
    {
        IQueryable<Player?> queryable = context.Set<Player>();

        queryable = queryable.Where(player => player != null && player.GameId == gameId);

        if (!enableTracking)
            queryable.AsNoTracking();

        return await queryable.ToPaginateAsync(index, size, 0);
    }
}