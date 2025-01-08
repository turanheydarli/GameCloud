using Microsoft.AspNetCore.Mvc;
using GameCloud.Application.Features.Users;
using GameCloud.Application.Common.Requests;
using GameCloud.Application.Features.Developers;
using GameCloud.Application.Features.Developers.Requests;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Games.Requests;
using GameCloud.Application.Features.ImageDocuments.Requests;
using GameCloud.Application.Features.Users.Requests;
using GameCloud.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace GameCloud.WebAPI.Controllers.V1;

[Route("api/v1/[controller]")]
public class DevelopersController(
    IDeveloperService developerService,
    IGameService gameService,
    IUserService userService) : BaseController
{
    #region Public Endpoints

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] PageableRequest request)
    {
        return Ok(await developerService.GetAllAsync(request));
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

    #endregion

    #region Authenticated Developer Endpoints

    [HttpGet("me")]
    [Authorize(Roles = "Developer")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserIdFromClaims();

        return Ok(await developerService.GetByUserIdAsync(userId));
    }

    [HttpPut("me")]
    [Authorize(Roles = "Developer")]
    public async Task<IActionResult> Update(DeveloperRequest request)
    {
        var userId = GetUserIdFromClaims();

        return Ok(await developerService.UpdateAsync(userId, request));
    }

    [HttpPut("me/photo")]
    [Authorize(Roles = "Developer")]
    public async Task<IActionResult> SetProfilePhoto(IFormFile image)
    {
        var userId = GetUserIdFromClaims();

        var request = new ImageUploadRequest
        {
            ImageStream = image.OpenReadStream(),
            FileName = image.FileName,
            ContentType = image.ContentType,
            Type = ImageType.GameIcon,
        };

        await developerService.SetProfilePhoto(userId, request);

        return Created();
    }

    #endregion
}