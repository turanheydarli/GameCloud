using AutoMapper;
using GameCloud.Application.Common.Paging;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Exceptions;
using GameCloud.Application.Features.Games;
using GameCloud.Application.Features.Games.Requests;
using GameCloud.Application.Features.Games.Responses;
using GameCloud.Application.Features.ImageDocuments;
using GameCloud.Application.Features.ImageDocuments.Requests;
using GameCloud.Application.Features.ImageDocuments.Responses;
using GameCloud.Application.Features.Players.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;
using GameCloud.Domain.Repositories;

namespace GameCloud.Business.Services;

public class GameService(
    IGameRepository gameRepository,
    IDeveloperRepository developerRepository,
    IGameKeyRepository gameKeyRepository,
    IImageService imageService,
    IPlayerRepository playerRepository,
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

    public async Task<PageableListResponse<PlayerResponse>> GetAllPlayersAsync(Guid gameId, PageableRequest request)
    {
        var game = await gameRepository.GetByIdAsync(gameId);
        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        var players =
            await playerRepository.GetAllByGameId(gameId, request.PageIndex, request.PageSize, false);

        return mapper.Map<PageableListResponse<PlayerResponse>>(players);
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

    public async Task<ImageResponse> SetGameImage(Guid gameId, ImageUploadRequest request)
    {
        var game = await gameRepository.GetByIdAsync(gameId);

        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        if (game.ImageId.HasValue)
        {
            await imageService.DeleteAsync(game.ImageId.Value);
        }

        request.Type = ImageType.GameIcon;
        var imageResponse = await imageService.UploadAsync(request);

        game.ImageId = imageResponse.Id;
        game.UpdatedAt = DateTime.UtcNow;

        await gameRepository.UpdateAsync(game);

        return imageResponse;
    }

    public async Task<ImageResponse> GetImageDetails(Guid gameId)
    {
        var game = await gameRepository.GetByIdAsync(gameId, IGameRepository.FullGameIncludes);

        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        if (game.Image is null)
        {
            throw new NotFoundException("Image not found for", gameId);
        }

        return mapper.Map<ImageResponse>(game.Image);
    }

    public async Task<GameResponse> GetById(Guid gameId)
    {
        var game = await gameRepository.GetByIdAsync(gameId, IGameRepository.DefaultIncludes);

        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        return mapper.Map<GameResponse>(game);
    }

    public async Task<PageableListResponse<GameResponse>> GetAllAsync(Guid userId, PageableRequest request)
    {
        var developer = await developerRepository.GetByUserIdAsync(userId);

        if (developer is null)
        {
            throw new NotFoundException("User", userId.ToString());
        }

        var games = await gameRepository.GetAllByDeveloperIdAsync(
            developerId: developer.Id,
            search: request.Search,
            ascending: request.IsAscending,
            page: request.PageIndex,
            size: request.PageSize,
            include: IGameRepository.DefaultIncludes);

        return mapper.Map<PageableListResponse<GameResponse>>(games);
    }

    public async Task DeleteAsync(Guid gameId)
    {
        var game = await gameRepository.GetByIdAsync(gameId);

        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        await gameRepository.DeleteAsync(game);
    }

    public async Task<GameResponse> UpdateAsync(Guid gameId, GameRequest request)
    {
        var game = await gameRepository.GetByIdAsync(gameId);

        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        game.Description = request.Description;
        game.Name = request.Name;
        game.IsEnabled = request.IsEnabled;
        game.UpdatedAt = DateTime.UtcNow;

        game = await gameRepository.UpdateAsync(game);

        return mapper.Map<GameResponse>(game);
    }

    public async Task RevokeKey(Guid gameId, string key)
    {
        var game = await gameRepository.GetByIdAsync(gameId);

        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        var gameKey = await gameKeyRepository.GetByApiKeyAsync(key);

        if (gameKey is null)
        {
            throw new NotFoundException("GameKey", gameId);
        }

        await gameKeyRepository.RevokeAsync(gameKey);
    }

    public async Task<ImageFileResponse> GetIconFile(Guid gameId, string variant = "original")
    {
        var game = await gameRepository.GetByIdAsync(gameId);

        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        var response = await imageService.GetImageFileByIdAsync(game.ImageId.Value, variant);

        return response;
    }

    public async Task<GameDetailResponse> GetGameDetailsAsync(Guid gameId)
    {
        var game = await gameRepository.GetByIdAsync(gameId, IGameRepository.FullGameIncludes);

        if (game is null)
        {
            throw new NotFoundException("Game", gameId);
        }

        var recentActivities = new List<GameActivityResponse>();
        if (game.Activities != null)
        {
            recentActivities = game.Activities
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .Select(a => new GameActivityResponse(
                    a.EventType,
                    a.Timestamp,
                    a.Details))
                .ToList();
        }

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var activePlayers = game.Players?.Count(p => p.UpdatedAt >= thirtyDaysAgo) ?? 0;

        return new GameDetailResponse(
            Id: game.Id,
            Name: game.Name,
            Description: game.Description,
            DeveloperId: game.DeveloperId,
            ImageId: game.ImageId,
            ImageUrl: game.Image?.Url,
            CreatedAt: game.CreatedAt,
            UpdatedAt: game.UpdatedAt,
            TotalPlayerCount: game.Players?.Count ?? 0,
            ActivePlayerCount: activePlayers,
            FunctionCount: game.Functions?.Count ?? 0,
            KeyCount: game.GameKeys?.Count ?? 0,
            RecentActivity: recentActivities
        );
    }
}