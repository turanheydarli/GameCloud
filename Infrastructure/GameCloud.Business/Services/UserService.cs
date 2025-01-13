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
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IDeveloperService _developerService;
        private readonly IPlayerService _playerService;
        private readonly IConfiguration _configuration;

        public UserService(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IDeveloperService developerService,
            IPlayerService playerService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _developerService = developerService;
            _playerService = playerService;
            _configuration = configuration;
        }

        #region Developer Methods

        public async Task<DeveloperResponse> RegisterDeveloperAsync(RegisterDeveloperRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException(request.Email);
            }

            var newUser = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create the user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await EnsureRoleExistsAsync("Developer");
            await _userManager.AddToRoleAsync(newUser, "Developer");

            var developerRequest = new DeveloperRequest(
                Id: Guid.Empty,
                UserId: newUser.Id,
                Name: request.Name,
                Email: request.Email
            );

            return await _developerService.CreateAsync(developerRequest);
        }

        public async Task<AuthResponse> LoginDeveloperAsync(LoginDeveloperRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new InvalidCredentialException("Username or password is incorrect.");
            }

            var token = GenerateJwtToken(user, "Developer");

            return new AuthResponse(
                UserId: user.Id,
                PlayerId: string.Empty,
                Email: user.Email,
                Token: token.Token,
                ExpiresAt: token.Expires
            );
        }

        #endregion

        #region Player Methods

        public async Task<AuthResponse> AuthenticatePlayerAsync(AuthPlayerRequest request)
        {
            string playerId;
            string? email = null;

            switch (request.Provider)
            {
                case AuthProvider.GooglePlay:
                    playerId = await ValidateGoogleIdTokenAsync(request.Token!);
                    break;

                case AuthProvider.iOS:
                    playerId = await ValidateAppleIdTokenAsync(request.Token!);
                    break;

                case AuthProvider.Unity:
                    playerId = await ValidateUnityTokenAsync(request.Token!);
                    break;

                case AuthProvider.Custom:
                    if (string.IsNullOrEmpty(request.PlayerId))
                    {
                        throw new ArgumentException("PlayerId must be provided for Custom provider.");
                    }

                    playerId = request.PlayerId!;
                    email = request.UserName;
                    break;

                default:
                    throw new NotImplementedException($"Auth provider '{request.Provider}' is not supported.");
            }

            var user = await _userManager.FindByNameAsync(playerId);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = playerId,
                    Email = email
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    throw new Exception($"Failed to create the player user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }

                await EnsureRoleExistsAsync("Player");
                await _userManager.AddToRoleAsync(user, "Player");

                var playerRequest = new PlayerRequest(Guid.Empty, request.Provider, playerId, email, user.Id);
                await _playerService.CreateAsync(playerRequest);
            }

            var token = GenerateJwtToken(user, "Player");
            var playerResponse = await _playerService.GetByUserIdAsync(user.Id);

            return new AuthResponse(
                UserId: user.Id,
                PlayerId: playerResponse.PlayerId,
                Email: user.Email,
                Token: token.Token,
                ExpiresAt: token.Expires
            );
        }

        #endregion

        #region Private Helper Methods

        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new AppRole { Name = roleName });
            }
        }

        private (string Token, DateTime Expires) GenerateJwtToken(AppUser user, string role)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expires = DateTime.UtcNow.AddHours(int.Parse(_configuration["Jwt:Expiration"] ?? "1"));

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

        private async Task<string> ValidateGoogleIdTokenAsync(string idToken)
        {
            // TODO: Implement real validation using Google.Apis.Auth
            await Task.CompletedTask;
            return "google-user-id-from-token";
        }

        private async Task<string> ValidateAppleIdTokenAsync(string idToken)
        {
            // TODO: Implement Apple Sign-In token validation
            await Task.CompletedTask;
            return "apple-user-id-from-token";
        }

        private async Task<string> ValidateUnityTokenAsync(string unityToken)
        {
            // TODO: Implement Unity authentication token validation
            await Task.CompletedTask;
            return "unity-user-id-from-token";
        }

        #endregion
    }
}
