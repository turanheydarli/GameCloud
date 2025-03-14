using AutoMapper;
using GameCloud.Application.Common.Paging;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Rooms;
using GameCloud.Application.Features.Rooms.Requests;
using GameCloud.Application.Features.Rooms.Responses;
using GameCloud.Domain.Entities.Rooms;
using GameCloud.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace GameCloud.Business.Services;

public class RoomService(
    IRoomRepository roomRepository,
    IGameRepository gameRepository,
    IGameContext gameContext,
    IMapper mapper,
    ILogger<RoomService> logger) : IRoomService
{
    public async Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request)
    {
        logger.LogInformation("Creating room for game ID: {GameId}", request.GameId);

        // Validate game exists
        var game = await gameRepository.GetByIdAsync(Guid.Parse(request.GameId));
        if (game is null)
        {
            throw new NotFoundException("Game", request.GameId);
        }

        // Create room config
        var roomConfig = mapper.Map<RoomConfig>(request.Config);

        // Create room
        var room = new Room
        {
            Name = request.Name,
            GameId = request.GameId,
            CreatorId = request.CreatorId,
            MaxPlayers = request.MaxPlayers,
            IsPrivate = request.IsPrivate,
            Label = request.Label,
            Metadata = request.Metadata ?? new Dictionary<string, string>(),
            PlayerIds = new List<string> { request.CreatorId.ToString() }, // Add creator as first player
            SpectatorIds = new List<string>(),
            State = RoomState.Created,
            CreatedAt = DateTime.UtcNow,
            TurnNumber = 0,
            Config = roomConfig
        };

        room = await roomRepository.CreateAsync(room);
        
        logger.LogInformation("Room created with ID: {RoomId}", room.Id);
        
        return mapper.Map<RoomResponse>(room);
    }

    public async Task<RoomResponse> GetRoomAsync(Guid roomId)
    {
        logger.LogInformation("Getting room with ID: {RoomId}", roomId);
        
        var room = await roomRepository.GetByIdAsync(roomId, IRoomRepository.DefaultIncludes);
        
        if (room is null)
        {
            throw new NotFoundException("Room", roomId);
        }
        
        return mapper.Map<RoomResponse>(room);
    }

    public async Task<PageableListResponse<RoomResponse>> GetRoomsAsync(Guid gameId, PageableRequest request)
    {
        logger.LogInformation("Getting rooms for game ID: {GameId}", gameId);
        
        var rooms = await roomRepository.GetAllByGameIdAsync(
            gameId.ToString(),
            request.Search,
            request.IsAscending,
            request.PageIndex,
            request.PageSize,
            true,
            IRoomRepository.DefaultIncludes);
        
        return mapper.Map<PageableListResponse<RoomResponse>>(rooms);
    }

    public async Task<RoomResponse> UpdateRoomStateAsync(UpdateRoomStateRequest request)
    {
        logger.LogInformation("Updating state for room ID: {RoomId}", request.RoomId);
        
        var room = await roomRepository.UpdateRoomStateAsync(
            request.RoomId,
            request.State,
            request.CurrentTurnUserId,
            request.TurnNumber,
            request.Metadata);
        
        if (room is null)
        {
            throw new NotFoundException("Room", request.RoomId);
        }
        
        return mapper.Map<RoomResponse>(room);
    }

    public async Task<JoinRoomResponse> JoinRoomAsync(JoinRoomRequest request)
    {
        logger.LogInformation("Player {UserId} joining room: {RoomId}", request.UserId, request.RoomId);
        
        var room = await roomRepository.GetByIdAsync(request.RoomId, IRoomRepository.DefaultIncludes);
        
        if (room is null)
        {
            return new JoinRoomResponse(false, "Room not found", null, -1);
        }
        
        // Check if room is full
        if (!request.AsSpectator && room.PlayerIds.Count >= room.MaxPlayers && room.MaxPlayers > 0)
        {
            return new JoinRoomResponse(false, "Room is full", null, -1);
        }
        
        // Check if spectators are allowed
        if (request.AsSpectator && !room.Config.AllowSpectators)
        {
            return new JoinRoomResponse(false, "Spectators are not allowed in this room", null, -1);
        }
        
        // Check if player is already in the room
        bool isAlreadyInRoom = room.PlayerIds.Contains(request.UserId) || room.SpectatorIds.Contains(request.UserId);
        int playerIndex = room.PlayerIds.IndexOf(request.UserId);
        
        if (!isAlreadyInRoom)
        {
            // Add player to the appropriate list
            if (request.AsSpectator)
            {
                room.SpectatorIds.Add(request.UserId);
            }
            else
            {
                room.PlayerIds.Add(request.UserId);
                playerIndex = room.PlayerIds.Count - 1;
            }
            
            await roomRepository.UpdateAsync(room);
            logger.LogInformation("Player {UserId} joined room: {RoomId}", request.UserId, request.RoomId);
        }
        
        return new JoinRoomResponse(
            true,
            null,
            mapper.Map<RoomResponse>(room),
            playerIndex);
    }

    public async Task<bool> LeaveRoomAsync(LeaveRoomRequest request)
    {
        logger.LogInformation("Player {UserId} leaving room: {RoomId}", request.UserId, request.RoomId);
        
        var success = await roomRepository.RemovePlayerFromRoomAsync(request.RoomId, request.UserId);
        
        if (success)
        {
            logger.LogInformation("Player {UserId} left room: {RoomId}", request.UserId, request.RoomId);
        }
        else
        {
            logger.LogWarning("Failed to remove player {UserId} from room: {RoomId}", request.UserId, request.RoomId);
        }
        
        return success;
    }

    public async Task<bool> KickPlayerAsync(KickPlayerRequest request)
    {
        logger.LogInformation("Kicking player {UserId} from room: {RoomId}", request.UserId, request.RoomId);
        
        var room = await roomRepository.GetByIdAsync(request.RoomId);
        
        if (room is null)
        {
            logger.LogWarning("Room not found: {RoomId}", request.RoomId);
            return false;
        }
        
        // Only the room creator can kick players
        if (room.CreatorId != request.RoomId)
        {
            logger.LogWarning("Only the room creator can kick players");
            return false;
        }
        
        var success = await roomRepository.RemovePlayerFromRoomAsync(request.RoomId, request.UserId);
        
        if (success)
        {
            logger.LogInformation("Player {UserId} was kicked from room: {RoomId}", request.UserId, request.RoomId);
            
            // Add kick reason to room metadata
            if (room.Metadata.ContainsKey("lastKickReason"))
            {
                room.Metadata["lastKickReason"] = request.Reason;
            }
            else
            {
                room.Metadata.Add("lastKickReason", request.Reason);
            }
            
            await roomRepository.UpdateAsync(room);
        }
        
        return success;
    }

    public async Task<bool> DeleteRoomAsync(Guid roomId)
    {
        logger.LogInformation("Deleting room: {RoomId}", roomId);
        
        var room = await roomRepository.GetByIdAsync(roomId);
        
        if (room is null)
        {
            logger.LogWarning("Room not found: {RoomId}", roomId);
            return false;
        }
        
        await roomRepository.DeleteAsync(room);
        logger.LogInformation("Room deleted: {RoomId}", roomId);
        
        return true;
    }

    public async Task<RoomResponse> UpdateRoomMetadataAsync(Guid roomId, Dictionary<string, string> metadata)
    {
        logger.LogInformation("Updating metadata for room: {RoomId}", roomId);
        
        var room = await roomRepository.GetByIdAsync(roomId, IRoomRepository.DefaultIncludes);
        
        if (room is null)
        {
            throw new NotFoundException("Room", roomId);
        }
        
        // Update metadata
        foreach (var kvp in metadata)
        {
            if (room.Metadata.ContainsKey(kvp.Key))
            {
                room.Metadata[kvp.Key] = kvp.Value;
            }
            else
            {
                room.Metadata.Add(kvp.Key, kvp.Value);
            }
        }
        
        room = await roomRepository.UpdateAsync(room);
        
        return mapper.Map<RoomResponse>(room);
    }

    public async Task<bool> PersistGameStateAsync(PersistGameStateRequest request)
    {
        logger.LogInformation("Persisting game state for room: {RoomId}", request.RoomId);
        
        var success = await roomRepository.PersistGameStateAsync(
            request.RoomId,
            request.StateData,
            request.CurrentTurnUserId,
            request.TurnNumber,
            request.Metadata);
        
        if (!success)
        {
            logger.LogWarning("Failed to persist game state for room: {RoomId}", request.RoomId);
        }
        
        return success;
    }
} 