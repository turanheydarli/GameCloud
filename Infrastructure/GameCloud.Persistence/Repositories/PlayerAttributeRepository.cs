using GameCloud.Domain.Entities;
using GameCloud.Domain.Entities.Matchmaking;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class PlayerAttributeRepository(GameCloudDbContext context) : IPlayerAttributeRepository
{
    public async Task<List<PlayerAttribute>> GetMatchingAttributesAsync(
        Guid playerId,
        IEnumerable<AttributeCriteria> criteria)
    {
        // 1) Flatten (Collection, Key) to a string
        var colKeySet = criteria
            .Select(c => c.Collection + "||" + c.Key)
            .ToHashSet();

        // 2) Query EF with a single "Contains" call
        return await context.Attributes
            .Where(a => a.PlayerId == playerId)
            .Where(a => colKeySet.Contains(a.Collection + "||" + a.Key))
            .ToListAsync();
    }

    public async Task<PlayerAttribute?> GetAsync(string username, string collection, string key)
    {
        return await context.Attributes
            .FirstOrDefaultAsync(attr =>
                attr.Username == username &&
                attr.Collection == collection &&
                attr.Key == key);
    }

    public async Task<IEnumerable<PlayerAttribute>> GetCollectionAsync(string username, string collection)
    {
        return await context.Attributes
            .Where(attr =>
                attr.Username == username &&
                attr.Collection == collection)
            .ToListAsync();
    }

    public async Task<PlayerAttribute> CreateAsync(PlayerAttribute attribute)
    {
        var existing = await GetAsync(attribute.Username, attribute.Collection, attribute.Key);

        context.Entry(attribute).State = EntityState.Added;
        await context.SaveChangesAsync();
        return attribute;
    }

    public async Task<PlayerAttribute> UpdateAsync(PlayerAttribute attribute)
    {
        var existing = await GetAsync(attribute.Username, attribute.Collection, attribute.Key);
        
        attribute.Version = (Convert.ToInt64(existing.Version, 16) + 1).ToString("x");
      
        context.Entry(existing).CurrentValues.SetValues(attribute);
        await context.SaveChangesAsync();
        return attribute;
    }

    public async Task DeleteAsync(string username, string collection, string key)
    {
        var attribute = await GetAsync(username, collection, key);

        context.Entry(attribute).State = EntityState.Deleted;
        await context.SaveChangesAsync();
    }
}