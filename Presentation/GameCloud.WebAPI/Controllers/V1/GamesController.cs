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
    #region Games

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Get([FromQuery] PageableRequest request)
    {
        return Ok(await gameService.GetAllAsync(request));
    }


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

    [HttpPut("{gameId}")]
    [Authorize(Policy = "OwnsGame")]
    public async Task<IActionResult> Update(Guid gameId, GameRequest request)
    {
        return Ok(await gameService.UpdateAsync(gameId, request));
    }

    [HttpDelete("{gameId}")]
    [Authorize(Roles = "OwnsGame")]
    public async Task<IActionResult> Delete(Guid gameId)
    {
        await gameService.DeleteAsync(gameId);
        return NoContent();
    }

    #endregion

    #region Game Keys

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


    [HttpDelete("{gameId}/keys/{key}")]
    [Authorize(Roles = "OwnsGame")]
    public async Task<IActionResult> DeleteKey(Guid gameId, string key)
    {
        await gameService.RevokeKey(gameId, key);
        return NoContent();
    }

    #endregion

    #region Functions

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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameId">Required for OwnsGame policy</param>
    /// <param name="functionId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{gameId}/functions/{functionId:guid}")]
    [Authorize(Policy = "OwnsGame")]
    public async Task<IActionResult> UpdateFunction(Guid gameId, Guid functionId, FunctionRequest request)
    {
        return Ok(await functionService.UpdateAsync(functionId, request));
    }

    [HttpDelete("{gameId}/functions/{functionId:guid}")]
    [Authorize(Roles = "OwnsGame")]
    public async Task<IActionResult> DeleteFunction(Guid gameId, Guid functionId)
    {
        await functionService.DeleteAsync(functionId);
        return NoContent();
    }

    #endregion
}