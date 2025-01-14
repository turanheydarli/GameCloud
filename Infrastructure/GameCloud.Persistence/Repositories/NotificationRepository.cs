using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class NotificationRepository(GameCloudDbContext context) : INotificationRepository
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

    public async Task<Player> UpdateAsync(Player player)
    {
        context.Entry(player).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return player;
    }

    public Task<Notification> CreateAsync(Notification notification)
    {
        throw new NotImplementedException();
    }

    public async Task CreateManyAsync(IEnumerable<Notification> notifications)
    {
        foreach (var notification in notifications)
        {
            context.Entry(notification).State = EntityState.Added;
        }

        await context.SaveChangesAsync();
    }

    public async Task<IPaginate<Notification>> GetNotificationsByPlayerAsync(string username, NotificationStatus status,
        int index = 0, int size = 10)
    {
        throw new NotImplementedException();
        // IQueryable<Notification?> queryable = context.Set<Notification>();
        //
        // queryable = queryable.Where(notification =>
        //     notification != null && notification.To == playerId && notification.Status == status);

        // return await queryable.ToPaginateAsync(index, size, 0);
    }
}