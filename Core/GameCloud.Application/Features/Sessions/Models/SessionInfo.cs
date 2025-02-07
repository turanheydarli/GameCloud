namespace GameCloud.Application.Features.Sessions.Models;

public class SessionInfo
{
    public Guid PlayerId { get; set; }
    public string SessionId { get; set; }
    public string DeviceId { get; set; }
    public DateTime ExpiresAt { get; set; }
}