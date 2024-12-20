using AutoMapper;
using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Games.Requests;
using GameCloud.Application.Features.Games.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class GameService(
    IGameRepository gameRepository,
    IDeveloperRepository developerRepository,
    IGameKeyRepository gameKeyRepository,
    IGameContext gameContext,
    IMapper mapper) : IGameService
{
    public async Task<PageableListResponse<GameKeyResponse>> GetAllKeysAsync(Guid gameId, PageableRequest request)
    {
        var game = await gameRepository.GetByIdAsync(gameId);
        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        var gameKeys = await gameRepository.GetAllKeysAsync(gameId, request.PageIndex, request.PageSize);
        return mapper.Map<PageableListResponse<GameKeyResponse>>(gameKeys);
    }

    public async Task<GameResponse> CreateGameAsync(GameRequest request, Guid userId)
    {
        var developer = await developerRepository.GetByUserIdAsync(userId);
        if (developer == null)
        {
            throw new NotFoundException("Developer", userId.ToString());
        }

        var game = new Game
        {
            Description = request.Description,
            Name = request.Name,
            DeveloperId = developer.Id,
            IsEnabled = true
        };

        game = await gameRepository.CreateAsync(game);
        return mapper.Map<GameResponse>(game);
    }

    public async Task<GameKeyResponse> CreateGameKey(Guid gameId)
    {
        var game = await gameRepository.GetByIdAsync(gameId);
        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        var gameKey = new GameKey
        {
            GameId = gameId,
            ApiKey = $"{Guid.NewGuid()}".Split("-")[0],
            Status = GameKeyStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        game.GameKeys = new List<GameKey> { gameKey };
        await gameRepository.UpdateAsync(game);

        return mapper.Map<GameKeyResponse>(gameKey);
    }

    public async Task<GameResponse> GetById(Guid gameId)
    {
        var game = await gameRepository.GetByIdAsync(gameId);
        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        throw new NotImplementedException();
    }
}