using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Games.Requests;
using GameCloud.Application.Features.Games.Responses;
using GameCloud.Application.Features.ImageDocuments.Requests;
using GameCloud.Application.Features.ImageDocuments.Responses;
using GameCloud.Application.Features.Players.Responses;

namespace GameCloud.Application.Features.Games;

public interface IGameService
{
    Task<PageableListResponse<GameKeyResponse>> GetAllKeysAsync(Guid gameId, PageableRequest request);
    Task<PageableListResponse<PlayerResponse>> GetAllPlayersAsync(Guid gameId, PageableRequest request);
    Task<GameResponse> CreateGameAsync(GameRequest request, Guid userId);
    Task<GameKeyResponse> CreateGameKey(Guid gameId);
    Task<ImageResponse> SetGameImage(Guid gameId, ImageUploadRequest request);
    Task<ImageResponse> GetImageDetails(Guid gameId);
    Task<GameResponse> GetById(Guid gameId);
    Task<PageableListResponse<GameResponse>> GetAllAsync(Guid userId, PageableRequest request);
    Task DeleteAsync(Guid gameId);
    Task<GameResponse> UpdateAsync(Guid gameId, GameRequest request);
    Task RevokeKey(Guid gameId, string key);
    Task<ImageFileResponse> GetIconFile(Guid gameId, string? variant = "original");
}