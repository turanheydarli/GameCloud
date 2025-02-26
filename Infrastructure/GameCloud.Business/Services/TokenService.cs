using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameCloud.Application.Features.Players;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GameCloud.Business.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    private readonly string? _secretKey = configuration["Jwt:Key"] ?? throw new NullReferenceException();
    private readonly string? _issuer = configuration["Jwt:Issuer"] ?? throw new NullReferenceException();
    private readonly string? _audience = configuration["Jwt:Audience"] ?? throw new NullReferenceException();

    public async Task<string> GenerateTokenAsync(TokenGenerationRequest request)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, request.PlayerId.ToString()),
            new Claim(ClaimTypes.Name, request.Username),
            new Claim("username", request.Username),
            new Claim("sid", request.SessionId),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        if (!string.IsNullOrEmpty(request.DeviceId))
        {
            claims.Add(new Claim("did", request.DeviceId));
        }

        if (!string.IsNullOrEmpty(request.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, request.Role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: request.ExpiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public async Task<string> GenerateRefreshTokenAsync()
    {
        return await Task.FromResult(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return await Task.FromResult(true);
        }
        catch
        {
            return await Task.FromResult(false);
        }
    }
}