using GameCloud.Application.Common.Requests;
using GameCloud.Application.Features.Functions;
using GameCloud.Application.Features.Functions.Requests;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Games.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Controllers.V1;

[Route("api/v1/[controller]")]
public class GamesController(
    IGameService gameService,
    IFunctionService functionService) : BaseController
{
    [Authorize(Policy = "OwnsGame")]
    [HttpGet("{gameId:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid gameId)
    {
        return Ok(await gameService.GetById(gameId));
    }

    [HttpPost]
    [Authorize(Roles = "Developer")]
    public async Task<IActionResult> Create([FromBody] GameRequest request)
    {
        var userId = GetUserIdFromClaims();
        return Ok(await gameService.CreateGameAsync(request, userId));
    }

    [Authorize(Policy = "OwnsGame")]
    [HttpGet("{gameId:guid}/keys")]
    public async Task<IActionResult> GetKeys([FromRoute] Guid gameId, [FromQuery] PageableRequest request)
    {
        return Ok(await gameService.GetAllKeysAsync(gameId, request));
    }

    [Authorize(Policy = "OwnsGame")]
    [HttpPost("{gameId:guid}/keys")]
    public async Task<IActionResult> CreateKey([FromRoute] Guid gameId)
    {
        return Ok(await gameService.CreateGameKey(gameId));
    }


    [Authorize(Policy = "OwnsGame")]
    [HttpPost("{gameId:guid}/functions")]
    public async Task<IActionResult> CreateFunction([FromRoute] Guid gameId, [FromBody] FunctionRequest request)
    {
        var userId = GetUserIdFromClaims();
        return Ok(await functionService.CreateFunctionAsync(gameId, request, userId));
    }

    [Authorize(Policy = "OwnsGame")]
    [HttpGet("{gameId:guid}/functions")]
    public async Task<IActionResult> GetFunctions([FromRoute] Guid gameId, [FromQuery] PageableRequest request)
    {
        return Ok(await functionService.GetFunctionsAsync(gameId, request));
    }
}