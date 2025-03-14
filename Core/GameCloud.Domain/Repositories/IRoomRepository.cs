using System.Linq.Expressions;
using GameCloud.Domain.Entities.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace GameCloud.Domain.Repositories;

public interface IRoomRepository
{
    public static readonly Func<IQueryable<Room>, IIncludableQueryable<Room, object>> DefaultIncludes =
        query => query
            .Include(r => r.Config);

    Task<IPaginate<Room>> GetAllAsync(int index = 0, int size = 10, bool enableTracking = true);

    Task<IPaginate<Room>> GetAllByGameIdAsync(
        string gameId,
        string? search = null,
        bool ascending = true,
        int page = 0,
        int size = 10, 
        bool enableTracking = true,
        Func<IQueryable<Room>, IIncludableQueryable<Room, object>>? include = null);

    Task<Room> CreateAsync(Room room);
    
    Task<Room?> GetByIdAsync(
        Guid id, 
        Func<IQueryable<Room>, IIncludableQueryable<Room, object>>? include = null);
    
    Task<Room> UpdateAsync(Room room);
    
    Task DeleteAsync(Room room);
    
    Task<Room?> GetByPlayerIdAsync(
        string playerId, 
        Func<IQueryable<Room>, IIncludableQueryable<Room, object>>? include = null);
    
    Task<List<Room>> GetActiveRoomsByGameIdAsync(
        string gameId, 
        Func<IQueryable<Room>, IIncludableQueryable<Room, object>>? include = null);
    
    Task<bool> IsPlayerInRoomAsync(Guid roomId, string playerId);
    
    Task<bool> AddPlayerToRoomAsync(Guid roomId, string playerId, bool asSpectator = false);
    
    Task<bool> RemovePlayerFromRoomAsync(Guid roomId, string playerId);
    
    Task<Room?> UpdateRoomStateAsync(
        Guid roomId, 
        RoomState state, 
        string? currentTurnUserId = null, 
        int? turnNumber = null,
        Dictionary<string, string>? metadata = null);
    
    Task<bool> PersistGameStateAsync(
        Guid roomId, 
        byte[] stateData, 
        string currentTurnUserId, 
        int turnNumber,
        Dictionary<string, string> metadata);
} 