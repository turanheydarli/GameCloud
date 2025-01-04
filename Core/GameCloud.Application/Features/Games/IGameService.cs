using GameCloud.Application.Common.Requests;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Games.Requests;
using GameCloud.Application.Features.Games.Responses;

namespace GameCloud.Application.Features.Games;

public interface IGameService
{
    Task<PageableListResponse<GameKeyResponse>> GetAllKeysAsync(Guid gameId, PageableRequest request);
    Task<GameResponse> CreateGameAsync(GameRequest request, Guid userId);
    Task<GameKeyResponse> CreateGameKey(Guid gameId);
    Task<GameResponse> GetById(Guid gameId);
    Task<PageableListResponse<GameResponse>> GetAllAsync(PageableRequest request);
    Task DeleteAsync(Guid gameId);
    Task<GameResponse> UpdateAsync(Guid gameId, GameRequest request);
    Task RevokeKey(Guid gameId, string key);
}