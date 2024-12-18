using GameCloud.Application.Features.Games.Requests;
using GameCloud.Application.Features.Games.Responses;

namespace GameCloud.Application.Features.Games;

public interface IGameService
{
    Task<GameResponse> GetGameByIdAsync(Guid id);
    Task<IEnumerable<GameResponse>> GetAllGamesAsync();
    Task<GameResponse> CreateGameAsync(GameRequest request);
    Task<GameResponse> UpdateGameAsync(Guid id, GameRequest request);
    Task<bool> DeleteGameAsync(Guid id);
}