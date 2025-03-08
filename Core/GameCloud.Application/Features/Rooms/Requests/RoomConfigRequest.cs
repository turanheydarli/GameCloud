namespace GameCloud.Application.Features.Rooms.Requests;

public record RoomConfigRequest(
    int MinPlayers,
    int MaxPlayers,
    int TurnTimerInSeconds,
    bool AllowSpectators,
    bool PersistState,
    Dictionary<string, string> CustomConfig
); 