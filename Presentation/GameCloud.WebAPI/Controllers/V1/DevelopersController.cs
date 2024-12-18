using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using GameCloud.Application.Features.Users;
using GameCloud.Application.Common.Requests;
using GameCloud.Application.Features.Developers;
using GameCloud.Application.Features.Developers.Requests;
using GameCloud.Application.Features.Users.Requests;
using Microsoft.AspNetCore.Authorization;

namespace GameCloud.WebAPI.Controllers.V1;

[Route("api/v1/[controller]")]
public class DevelopersController(
    IDeveloperService developerService,
    IUserService userService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] PageableRequest request)
    {
        return Success(await developerService.GetAllAsync(request));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Developer")]
    public async Task<IActionResult> GetMe()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdString))
        {
            throw new UnauthorizedAccessException("Invalid token or missing user identifier.");
        }

        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            throw new UnauthorizedAccessException("Invalid token user identifier.");
        }

        var developer = await developerService.GetByUserIdAsync(userId);
        if (developer == null)
        {
            throw new BadHttpRequestException("User not found.");
        }

        return Success(developer);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        return Success(await developerService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDeveloperRequest request)
    {
        return Success(await userService.RegisterDeveloperAsync(request));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDeveloperRequest request)
    {
        return Success(await userService.LoginDeveloperAsync(request));
    }

    [HttpPut]
    public async Task<IActionResult> Update(DeveloperRequest request)
    {
        throw new NotImplementedException();
    }
}