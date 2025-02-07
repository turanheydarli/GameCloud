using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameCloud.Application.Features.Players;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GameCloud.Business.Services;

public class TokenService : ITokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public TokenService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"];
        _issuer = configuration["Jwt:Issuer"];
        _audience = configuration["Jwt:Audience"];
    }

    public async Task<string> GenerateTokenAsync(TokenGenerationRequest request)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.PlayerId.ToString()),
            new Claim("username", request.Username),
            new Claim("sid", request.SessionId),
            new Claim(JwtRegisteredClaimNames.Iat, request.IssuedAt.ToString("O")),
            new Claim(JwtRegisteredClaimNames.Exp, request.ExpiresAt.ToString("O"))
        };

        if (!string.IsNullOrEmpty(request.DeviceId))
        {
            claims = claims.Append(new Claim("did", request.DeviceId)).ToArray();
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: request.ExpiresAt,
            signingCredentials: creds
        );

        return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
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