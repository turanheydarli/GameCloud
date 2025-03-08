namespace GameCloud.Application.Features.Rooms.Requests;

public record KickPlayerRequest(
    Guid RoomId,
    string UserId,
    string Reason
); 