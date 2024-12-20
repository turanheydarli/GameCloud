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
        return Ok(await developerService.GetAllAsync(request));
    }

    [HttpGet("me")]
    [Authorize(Roles = "Developer")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserIdFromClaims();

        var developer = await developerService.GetByUserIdAsync(userId);
        if (developer == null)
        {
            throw new BadHttpRequestException("User not found.");
        }

        return Ok(developer);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok(await developerService.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDeveloperRequest request)
    {
        return Ok(await userService.RegisterDeveloperAsync(request));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDeveloperRequest request)
    {
        return Ok(await userService.LoginDeveloperAsync(request));
    }

    [HttpPut("me")]
    [Authorize(Roles = "Developer")]
    public async Task<IActionResult> Update(DeveloperRequest request)
    {
        throw new NotImplementedException();
    }
}