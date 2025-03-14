using GameCloud.Domain.Entities.Rooms;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using GameCloud.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace GameCloud.Persistence.Repositories;

public class RoomRepository(GameCloudDbContext context) : IRoomRepository
{
    public async Task<IPaginate<Room>> GetAllAsync(int index = 0, int size = 10, bool enableTracking = true)
    {
        IQueryable<Room> queryable = context.Set<Room>();
        
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
            
        return await queryable.ToPaginateAsync(index, size, 0);
    }

    public async Task<IPaginate<Room>> GetAllByGameIdAsync(
        string gameId,
        string? search = null,
        bool ascending = true,
        int page = 0,
        int size = 10,
        bool enableTracking = true,
        Func<IQueryable<Room>, IIncludableQueryable<Room, object>>? include = null)
    {
        IQueryable<Room> queryable = context.Set<Room>();

        queryable = queryable.Where(r => r.GameId == gameId);
        
        if (include != null)
            queryable = include(queryable);

        if (!enableTracking)
            queryable = queryable.AsNoTracking();

        queryable = ascending
            ? queryable.OrderBy(r => r.CreatedAt)
            : queryable.OrderByDescending(r => r.CreatedAt);

        if (!string.IsNullOrEmpty(search))
        {
            queryable = queryable.Where(r =>
                r.Name.Contains(search) ||
                r.Label.Contains(search));
        }

        return await queryable.ToPaginateAsync(page, size, 0);
    }

    public async Task<Room> CreateAsync(Room room)
    {
        context.Entry(room).State = EntityState.Added;
        await context.SaveChangesAsync();
        return room;
    }

    public async Task<Room?> GetByIdAsync(
        Guid id,
        Func<IQueryable<Room>, IIncludableQueryable<Room, object>>? include = null)
    {
        IQueryable<Room> queryable = context.Set<Room>();

        queryable = queryable.Where(r => r.Id == id);

        if (include != null)
            queryable = include(queryable);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<Room> UpdateAsync(Room room)
    {
        context.Entry(room).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return room;
    }

    public async Task DeleteAsync(Room room)
    {
        context.Entry(room).State = EntityState.Deleted;
        await context.SaveChangesAsync();
    }

    public async Task<Room?> GetByPlayerIdAsync(
        string playerId,
        Func<IQueryable<Room>, IIncludableQueryable<Room, object>>? include = null)
    {
        IQueryable<Room> queryable = context.Set<Room>();

        queryable = queryable.Where(r => 
            r.PlayerIds.Contains(playerId) || 
            r.SpectatorIds.Contains(playerId));

        if (include != null)
            queryable = include(queryable);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<List<Room>> GetActiveRoomsByGameIdAsync(
        string gameId,
        Func<IQueryable<Room>, IIncludableQueryable<Room, object>>? include = null)
    {
        IQueryable<Room> queryable = context.Set<Room>();

        queryable = queryable.Where(r => 
            r.GameId == gameId && 
            (r.State == RoomState.Created || r.State == RoomState.Started));

        if (include != null)
            queryable = include(queryable);

        return await queryable.ToListAsync();
    }

    public async Task<bool> IsPlayerInRoomAsync(Guid roomId, string playerId)
    {
        var room = await context.Set<Room>()
            .Where(r => r.Id == roomId)
            .Select(r => new { r.PlayerIds, r.SpectatorIds })
            .FirstOrDefaultAsync();

        if (room == null)
            return false;

        return room.PlayerIds.Contains(playerId) || room.SpectatorIds.Contains(playerId);
    }

    public async Task<bool> AddPlayerToRoomAsync(Guid roomId, string playerId, bool asSpectator = false)
    {
        var room = await context.Set<Room>()
            .Include(r => r.Config)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
            return false;

        // Check if room is full
        if (!asSpectator && room.PlayerIds.Count >= room.MaxPlayers && room.MaxPlayers > 0)
            return false;

        // Check if spectators are allowed
        if (asSpectator && !room.Config.AllowSpectators)
            return false;

        // Check if player is already in the room
        if (room.PlayerIds.Contains(playerId) || room.SpectatorIds.Contains(playerId))
            return true; // Player is already in the room

        // Add player to the appropriate list
        if (asSpectator)
            room.SpectatorIds.Add(playerId);
        else
            room.PlayerIds.Add(playerId);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePlayerFromRoomAsync(Guid roomId, string playerId)
    {
        var room = await context.Set<Room>()
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
            return false;

        bool removed = false;

        // Remove from players if present
        if (room.PlayerIds.Contains(playerId))
        {
            room.PlayerIds.Remove(playerId);
            removed = true;
        }

        // Remove from spectators if present
        if (room.SpectatorIds.Contains(playerId))
        {
            room.SpectatorIds.Remove(playerId);
            removed = true;
        }

        // If this was the creator and there are other players, assign a new creator
        if (removed && room.CreatorId.ToString() == playerId && room.PlayerIds.Count > 0)
        {
            room.CreatorId = Guid.Parse(room.PlayerIds[0]);
        }

        await context.SaveChangesAsync();
        return removed;
    }

    public async Task<Room?> UpdateRoomStateAsync(
        Guid roomId,
        RoomState state,
        string? currentTurnUserId = null,
        int? turnNumber = null,
        Dictionary<string, string>? metadata = null)
    {
        var room = await context.Set<Room>()
            .Include(r => r.Config)
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room == null)
            return null;

        room.State = state;

        if (currentTurnUserId != null)
            room.CurrentTurnUserId = currentTurnUserId;

        if (turnNumber.HasValue)
            room.TurnNumber = turnNumber.Value;

        if (metadata != null && metadata.Count > 0)
        {
            foreach (var kvp in metadata)
            {
                if (room.Metadata.ContainsKey(kvp.Key))
                    room.Metadata[kvp.Key] = kvp.Value;
                else
                    room.Metadata.Add(kvp.Key, kvp.Value);
            }
        }

        await context.SaveChangesAsync();
        return room;
    }

    public async Task<bool> PersistGameStateAsync(
        Guid roomId,
        byte[] stateData,
        string currentTurnUserId,
        int turnNumber,
        Dictionary<string, string> metadata)
    {
        throw new NotImplementedException();
        
        // var room = await context.Set<Room>()
        //     .FirstOrDefaultAsync(r => r.Id == roomId);
        //
        // if (room == null)
        //     return false;
        //
        // // Create or update the game state entity
        // var gameState = await context.Set<GameState>()
        //     .FirstOrDefaultAsync(gs => gs.RoomId == roomId);
        //
        // if (gameState == null)
        // {
        //     gameState = new GameState
        //     {
        //         RoomId = roomId,
        //         StateData = stateData,
        //         CurrentTurnUserId = currentTurnUserId,
        //         TurnNumber = turnNumber,
        //         Metadata = metadata,
        //         CreatedAt = DateTime.UtcNow,
        //         UpdatedAt = DateTime.UtcNow
        //     };
        //     
        //     context.Set<GameState>().Add(gameState);
        // }
        // else
        // {
        //     gameState.StateData = stateData;
        //     gameState.CurrentTurnUserId = currentTurnUserId;
        //     gameState.TurnNumber = turnNumber;
        //     gameState.Metadata = metadata;
        //     gameState.UpdatedAt = DateTime.UtcNow;
        //     
        //     context.Entry(gameState).State = EntityState.Modified;
        // }
        //
        // // Update the room with the latest turn information
        // room.CurrentTurnUserId = currentTurnUserId;
        // room.TurnNumber = turnNumber;
        //
        // // Update room metadata
        // foreach (var kvp in metadata)
        // {
        //     if (room.Metadata.ContainsKey(kvp.Key))
        //         room.Metadata[kvp.Key] = kvp.Value;
        //     else
        //         room.Metadata.Add(kvp.Key, kvp.Value);
        // }
        //
        // await context.SaveChangesAsync();
        // return true;
    }
} 