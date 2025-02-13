namespace GameCloud.Application.Features.Players;

public class TokenGenerationRequest
{
    public Guid PlayerId { get; set; }
    public string Username { get; set; }
    public string SessionId { get; set; }
    public string DeviceId { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
        public string? Role { get; set; }

}

public interface ITokenService
{
    Task<string> GenerateTokenAsync(TokenGenerationRequest request);
    Task<string> GenerateRefreshTokenAsync();
    Task<bool> ValidateTokenAsync(string token);
}