using GameCloud.Application.Common.Paging;
using GameCloud.Application.Features.Actions;
using GameCloud.Application.Features.Actions.Requests;
using GameCloud.Application.Features.Actions.Responses;
using GameCloud.Application.Features.Functions;
using GameCloud.Application.Features.Functions.Requests;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Games.Requests;
using GameCloud.Application.Features.ImageDocuments.Requests;
using GameCloud.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Controllers.V1;

[Route("api/v1/[controller]")]
public class GamesController(
    IGameService gameService,
    IFunctionService functionService,
    IActionService actionService) : BaseController
{
    #region Games

    [HttpGet]
    [Authorize(Roles = "Developer")]
    public async Task<IActionResult> Get([FromQuery] PageableRequest request)
    {
        var userId = GetUserIdFromClaims();

        return Ok(await gameService.GetAllAsync(userId, request));
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


    [HttpPost("{gameId:guid}/icon")]
    public async Task<IActionResult> SetImage([FromRoute] Guid gameId, IFormFile image)
    {
        var request = new ImageUploadRequest
        {
            ImageStream = image.OpenReadStream(),
            FileName = image.FileName,
            ContentType = image.ContentType,
            Type = ImageType.GameIcon,
        };

        return Ok(await gameService.SetGameImage(gameId, request));
    }

    [HttpGet("{gameId:guid}/images")]
    public async Task<IActionResult> GetImages(Guid gameId)
    {
        return Ok(await gameService.GetImageDetails(gameId));
    }

    [HttpGet("{gameId:guid}/details")]
    [Authorize(Policy = "OwnsGame")]
    public async Task<IActionResult> GetDetails(Guid gameId)
    {
        return Ok(await gameService.GetGameDetailsAsync(gameId));
    }

    [HttpGet("{gameId:guid}/icon")]
    public async Task<IActionResult> GetIcon(Guid gameId, [FromQuery] string? variant)
    {
        var image = await gameService.GetIconFile(gameId, variant);

        return File(image.Stream, image.ContentType, image.FileName);
    }

    [Authorize(Policy = "OwnsGame")]
    [HttpGet("{gameId:guid}/players")]
    public async Task<IActionResult> GetPlayers(Guid gameId, PageableRequest request)
    {
        return Ok(await gameService.GetAllPlayersAsync(gameId, request));
    }

    [HttpPut("{gameId}")]
    [Authorize(Policy = "OwnsGame")]
    public async Task<IActionResult> Update(Guid gameId, GameRequest request)
    {
        return Ok(await gameService.UpdateAsync(gameId, request));
    }

    [HttpDelete("{gameId}")]
    [Authorize(Policy = "OwnsGame")]
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
    [HttpGet("{gameId:guid}/functions/{functionId:guid}")]
    public async Task<IActionResult> GetFunction([FromRoute] Guid gameId, [FromRoute] Guid functionId)
    {
        return Ok(await functionService.GetById(gameId, functionId));
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

    [Authorize(Policy = "OwnsGame")]
    [HttpGet("{gameId:guid}/functions/{functionId:guid}/test/logs")]
    public async Task<IActionResult> GetTestedFunctionLogs(
        Guid gameId,
        Guid functionId,
        [FromQuery] PageableRequest request)
    {
        return Ok(await actionService.GetTestedFunctionLogs(functionId, request));
    }

    [Authorize(Policy = "OwnsGame")]
    [HttpPost("{gameId:guid}/functions/{functionId:guid}/test/{actionId:guid}")]
    public async Task<IActionResult> GetFunctionLog(Guid gameId, Guid functionId, Guid actionId)
    {
        return Ok(await actionService.GetFunctionLogByActionId(actionId));
    }

    [Authorize(Policy = "OwnsGame")]
    [HttpPost("{gameId:guid}/functions/{functionId:guid}/test")]
    public async Task<IActionResult> TestFunction(Guid gameId, Guid functionId, [FromBody] ActionRequest request)
    {
        var userId = GetUserIdFromClaims();
        return Ok(await actionService.ExecuteActionAsync(Guid.Empty, userId, request, true));
    }

    [HttpPut("{gameId}/functions/{functionId:guid}")]
    [Authorize(Policy = "OwnsGame")]
    public async Task<IActionResult> UpdateFunction(Guid gameId, Guid functionId, FunctionRequest request)
    {
        return Ok(await functionService.UpdateAsync(functionId, request));
    }

    [HttpPut("{gameId}/functions/{functionId:guid}/toggle")]
    [Authorize(Policy = "OwnsGame")]
    public async Task<IActionResult> UpdateFunction(Guid gameId, Guid functionId, bool isEnabled)
    {
        return Ok(await functionService.ToggleFunction(gameId, functionId, isEnabled));
    }

    [HttpDelete("{gameId}/functions/{functionId:guid}")]
    [Authorize(Policy = "OwnsGame")]
    public async Task<IActionResult> DeleteFunction(Guid gameId, Guid functionId)
    {
        await functionService.DeleteAsync(functionId);
        return NoContent();
    }

    #endregion

    #region Functions Stats

    [Authorize(Policy = "OwnsGame")]
    [HttpGet("{gameId:guid}/functions/{functionId:guid}/stats")]
    public async Task<IActionResult> GetFunctionStats(
        [FromRoute] Guid gameId,
        [FromRoute] Guid functionId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var start = from ?? DateTime.UtcNow.AddHours(-24);
        var end = to ?? DateTime.UtcNow;

        return Ok(await actionService.GetFunctionStatsAsync(
            functionId,
            new DateTimeRange(start, end)
        ));
    }

    [Authorize(Policy = "OwnsGame")]
    [HttpGet("{gameId:guid}/functions/stats")]
    public async Task<IActionResult> GetGameFunctionsStats(
        [FromRoute] Guid gameId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var start = from ?? DateTime.UtcNow.AddHours(-24);
        var end = to ?? DateTime.UtcNow;

        return Ok(await actionService.GetGameFunctionsStatsAsync(
            gameId,
            new DateTimeRange(start, end)
        ));
    }

    #endregion
}