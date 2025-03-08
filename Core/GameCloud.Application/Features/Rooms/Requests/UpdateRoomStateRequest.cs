using GameCloud.Domain.Entities.Rooms;

namespace GameCloud.Application.Features.Rooms.Requests;

public record UpdateRoomStateRequest(
    Guid RoomId,
    RoomState State,
    string? CurrentTurnUserId = null,
    int? TurnNumber = null,
    Dictionary<string, string>? Metadata = null
); 