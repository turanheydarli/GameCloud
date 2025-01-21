using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Developers;
using GameCloud.Application.Features.Developers.Requests;
using GameCloud.Application.Features.Developers.Responses;
using GameCloud.Application.Features.Players;
using GameCloud.Application.Features.Users.Requests;
using GameCloud.Application.Features.Users;
using GameCloud.Application.Features.Players.Requests;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace GameCloud.Business.Services
{
    public class UserService(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        IDeveloperService developerService,
        IPlayerService playerService,
        IConfiguration configuration)
        : IUserService
    {
        #region Developer Methods

        public async Task<DeveloperResponse> RegisterDeveloperAsync(RegisterDeveloperRequest request)
        {
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException(request.Email);
            }

            var newUser = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
            };

            var result = await userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                throw new Exception(
                    $"Failed to create the user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await EnsureRoleExistsAsync("Developer");
            await userManager.AddToRoleAsync(newUser, "Developer");

            var developerRequest = new DeveloperRequest(
                Id: Guid.Empty,
                UserId: newUser.Id,
                Name: request.Name,
                Email: request.Email
            );

            return await developerService.CreateAsync(developerRequest);
        }

        public async Task<AuthResponse> LoginDeveloperAsync(LoginDeveloperRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new InvalidCredentialException("Username or password is incorrect.");
            }

            var token = GenerateJwtToken(user, "Developer");

            return new AuthResponse(
                UserId: user.Id,
                Username: user.UserName,
                Email: user.Email,
                Token: token.Token,
                ExpiresAt: token.Expires
            );
        }

        #endregion

        #region Player Methods

        public async Task<AuthResponse> AuthenticatePlayerAsync(AuthPlayerRequest request)
        {
            string username = "";

            switch (request.Provider)
            {
                case AuthProvider.Custom:
                    username = request.UserName;
                    break;

                default:
                    throw new NotImplementedException($"Auth provider '{request.Provider}' is not supported.");
            }

            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = username
                };

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    throw new Exception(
                        $"Failed to create the player user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }

                await EnsureRoleExistsAsync("Player");
                await userManager.AddToRoleAsync(user, "Player");

                var playerRequest = new PlayerRequest(Guid.Empty, request.Provider, username, user.Id);
                await playerService.CreateAsync(playerRequest);
            }

            var token = GenerateJwtToken(user, "Player");
            var playerResponse = await playerService.GetByUserIdAsync(user.Id);

            return new AuthResponse(
                UserId: user.Id,
                Username: playerResponse.Username,
                Email: user.Email,
                Token: token.Token,
                ExpiresAt: token.Expires
            );
        }

        #endregion

        #region Private Helper Methods

        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole { Name = roleName });
            }
        }

        private (string Token, DateTime Expires) GenerateJwtToken(AppUser user, string role)
        {
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];
            var expires = DateTime.UtcNow.AddHours(int.Parse(configuration["Jwt:Expiration"] ?? "1"));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email ?? user.UserName),
                new Claim(ClaimTypes.Role, role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(securityToken);

            return (tokenString, expires);
        }

        #endregion
    }
}