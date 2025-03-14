namespace GameCloud.Application.Features.Rooms.Requests;

public record LeaveRoomRequest(
    Guid RoomId,
    string UserId,
    string SessionId
); 