using System.Text.Json;

namespace GameCloud.Domain.Entities.Rooms;

public class RoomConfig : BaseEntity
{
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public int TurnTimerInSeconds { get; set; }
    public bool AllowSpectators { get; set; }
    public bool PersistState { get; set; }
    public Dictionary<string, string> CustomConfig { get; set; } = new();
}