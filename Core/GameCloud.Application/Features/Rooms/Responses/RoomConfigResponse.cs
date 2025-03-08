namespace GameCloud.Application.Features.Rooms.Responses;

public record RoomConfigResponse(
    int MinPlayers,
    int MaxPlayers,
    int TurnTimerInSeconds,
    bool AllowSpectators,
    bool PersistState,
    Dictionary<string, string> CustomConfig
); 