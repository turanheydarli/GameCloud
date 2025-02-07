using GameCloud.Application.Common.Paging;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Functions.Responses;
using GameCloud.Application.Features.Players.Requests;
using GameCloud.Application.Features.Players.Responses;

namespace GameCloud.Application.Features.Players;

public interface IPlayerService
{
    Task<AuthenticationResponse> AuthenticateWithDeviceAsync(string deviceId, Dictionary<string, object> metadata = null);
    Task<AuthenticationResponse> AuthenticateWithCustomIdAsync(string customId, Dictionary<string, object> metadata = null, bool create = true);
    Task<AuthenticationResponse> RefreshSessionAsync(string refreshToken);
    
    Task<PageableListResponse<PlayerResponse>> GetAllAsync(PageableRequest request);
    Task<PlayerResponse> CreateAsync(PlayerRequest request);
    Task<PlayerResponse> GetByIdAsync(Guid id);
    Task<PlayerResponse> GetByUserIdAsync(Guid userId);
    Task<Dictionary<string, AttributeResponse>> GetAttributesAsync(string collection, string username);
    Task<AttributeResponse> GetAttributeAsync(string username, string collection, string key);
    Task SetAttributeAsync(string username, string collection, AttributeRequest request);
    Task RemoveAttributeAsync(string username, string collection, string key);
    Task ApplyAttributeUpdatesAsync(string username, IEnumerable<EntityAttributeUpdate> updates);
}

public class AuthenticationResponse
{
    public PlayerResponse Player { get; set; }
    public string Token { get; set; }           
    public string RefreshToken { get; set; }    
    public DateTime ExpiresAt { get; set; }    
    public string SessionId { get; set; }
    public Dictionary<string, string> Vars { get; set; } = new();  
    public string DeviceId { get; set; }                         
    public DateTime IssuedAt { get; set; }                       
}