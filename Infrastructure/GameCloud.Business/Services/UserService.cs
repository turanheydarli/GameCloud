using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Developers;
using GameCloud.Application.Features.Developers.Requests;
using GameCloud.Application.Features.Developers.Responses;
using GameCloud.Application.Features.Users.Requests;
using GameCloud.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using GameCloud.Application.Features.Users;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace GameCloud.Business.Services
{
    public class UserService(
        UserManager<AppUser?> userManager,
        RoleManager<AppRole> roleManager,
        IDeveloperService developerService,
        IConfiguration configuration
    ) : IUserService
    {
        public async Task<DeveloperResponse> RegisterDeveloperAsync(RegisterDeveloperRequest request)
        {
            AppUser? existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException($"Email '{request.Email}' already exists.");
            }

            var newUser = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
            };

            var result = await userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create the user: " +
                                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!await roleManager.RoleExistsAsync("Developer"))
            {
                await roleManager.CreateAsync(new AppRole { Name = "Developer" });
            }

            await userManager.AddToRoleAsync(newUser, "Developer");

            var developer = await developerService.CreateAsync(new DeveloperRequest(
                Guid.Empty,
                newUser.Id,
                request.Name,
                request.Email
            ));

            return developer;
        }

        public async Task<AuthResponse> LoginDeveloperAsync(LoginDeveloperRequest request)
        {
            AppUser? developer = await userManager.FindByEmailAsync(request.Email);
            if (developer == null)
            {
                throw new NotFoundException("Email or password is incorrect.");
            }

            bool validPassword = await userManager.CheckPasswordAsync(developer, request.Password);
            if (!validPassword)
            {
                throw new NotFoundException("Email or password is incorrect.");
            }

            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];
            var expires = DateTime.UtcNow.AddHours(int.Parse(configuration["Jwt:Expiration"] ?? "1"));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, developer.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, developer.Email),
                new Claim(ClaimTypes.NameIdentifier, developer.Id.ToString()),
                new Claim(ClaimTypes.Name, developer.Email),
                new Claim(ClaimTypes.Role, "Developer")
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

            return new AuthResponse(
                UserId: developer.Id,
                PlayerId: string.Empty,
                Email: developer.Email,
                Token: tokenString,
                ExpiresAt: expires
            );
        }
    }
}