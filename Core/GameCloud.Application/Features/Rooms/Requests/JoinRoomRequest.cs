namespace GameCloud.Application.Features.Rooms.Requests;

public record JoinRoomRequest(
    Guid RoomId,
    string UserId,
    string SessionId,
    bool AsSpectator = false
); 