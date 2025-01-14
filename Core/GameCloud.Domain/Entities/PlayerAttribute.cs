using System.Text.Json;

namespace GameCloud.Domain.Entities;

public class PlayerAttribute : BaseEntity
{
    public Guid PlayerId { get; set; }
    public string Username { get; set; }
    public string Collection { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string Version { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public JsonDocument PermissionRead { get; set; }
    public JsonDocument PermissionWrite { get; set; }

    public virtual Player Player { get; set; }
}