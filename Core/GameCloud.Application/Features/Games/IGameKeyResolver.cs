namespace GameCloud.Application.Features.Games;

public interface IGameKeyResolver
{
    Task<Guid> ResolveGameIdAsync(string gameKey);
}
