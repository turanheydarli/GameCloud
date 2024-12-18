namespace GameCloud.Application.Features.Games;

public class GameContext(Guid gameId) : IGameContext
{
    public Guid GameId { get; } = gameId;
}