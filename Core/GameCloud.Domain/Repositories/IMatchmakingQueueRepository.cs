using GameCloud.Domain.Entities.Matchmaking;

namespace GameCloud.Domain.Repositories;

public interface IMatchmakingQueueRepository
{
    Task<MatchmakingQueue?> GetByGameAndNameAsync(Guid gameId, string queueName);
    Task<MatchmakingQueue?> GetByIdAsync(Guid queueId);
    Task<IEnumerable<MatchmakingQueue>> GetAllAsync();
    Task CreateAsync(MatchmakingQueue queue);
    Task UpdateAsync(MatchmakingQueue queue);
    Task DeleteAsync(MatchmakingQueue queue);
}

public interface IMatchTicketRepository
{
    Task<MatchTicket?> GetByIdAsync(Guid ticketId);
    Task CreateAsync(MatchTicket ticket);
    Task UpdateAsync(MatchTicket ticket);
    Task DeleteAsync(MatchTicket ticket);

    Task<List<MatchTicket>> GetActiveTicketsAsync(Guid queueId);
    Task UpdateRangeAsync(List<MatchTicket> group);
}

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid matchId);
    Task CreateAsync(Match match);
    Task UpdateAsync(Match match);
}