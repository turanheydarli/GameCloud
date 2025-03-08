namespace GameCloud.Application.Features.Rooms.Requests;

public record PersistGameStateRequest(
    Guid RoomId,
    byte[] StateData,
    string CurrentTurnUserId,
    int TurnNumber,
    Dictionary<string, string> Metadata
); 