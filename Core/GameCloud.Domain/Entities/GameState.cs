using System.Text.Json;

namespace GameCloud.Domain.Entities;

public class GameState : BaseEntity
{
    public Guid SessionId { get; set; }
    public JsonDocument? StateData { get; set; }
    public JsonDocument? AppliedFunctions { get; set; }
    public JsonDocument? Deltas { get; set; }
    public int StateVersion { get; set; }
}