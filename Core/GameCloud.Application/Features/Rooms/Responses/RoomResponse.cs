using GameCloud.Domain.Entities.Rooms;

namespace GameCloud.Application.Features.Rooms.Responses;

public record RoomResponse(
    Guid Id,
    string Name,
    string GameId,
    Guid CreatorId,
    RoomState State,
    Dictionary<string, string> Metadata,
    List<string> PlayerIds,
    List<string> SpectatorIds,
    int MaxPlayers,
    DateTime CreatedAt,
    string? CurrentTurnUserId,
    int TurnNumber,
    bool IsPrivate,
    string Label,
    RoomConfigResponse Config
); 