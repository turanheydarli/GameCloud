using GameCloud.Domain.Entities.Rooms;

namespace GameCloud.Application.Features.Rooms.Requests;

public record CreateRoomRequest(
    string Name,
    string GameId,
    Guid CreatorId,
    int MaxPlayers,
    bool IsPrivate,
    string Label,
    Dictionary<string, string> Metadata,
    RoomConfigRequest Config
); 