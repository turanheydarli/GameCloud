namespace GameCloud.Application.Features.Rooms.Responses;

public record JoinRoomResponse(
    bool Success,
    string? Error,
    RoomResponse? Room,
    int PlayerIndex
); 