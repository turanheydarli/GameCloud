namespace GameCloud.Application.Exceptions;

public class MatchmakingException : ApplicationException
{
    public MatchmakingException(string message) : base(message)
    {
    }

    public MatchmakingException(string message, Exception inner) : base(message, inner)
    {
    }
}

public class MatchQueueNotFoundException : MatchmakingException
{
    public Guid GameId { get; }
    public string QueueName { get; }

    public MatchQueueNotFoundException(Guid gameId, string queueName)
        : base($"Match queue '{queueName}' not found for game {gameId}")
    {
        GameId = gameId;
        QueueName = queueName;
    }
}

public class MatchmakerFunctionNotFoundException : MatchmakingException
{
    public string FunctionName { get; }
    public Guid GameId { get; }

    public MatchmakerFunctionNotFoundException(string functionName, Guid gameId)
        : base($"Matchmaker function '{functionName}' not found for game {gameId}")
    {
        FunctionName = functionName;
        GameId = gameId;
    }
}

public class MatchTicketException : MatchmakingException
{
    public Guid TicketId { get; }

    public MatchTicketException(Guid ticketId, string message) : base(message)
    {
        TicketId = ticketId;
    }
}

public class PlayerPresenceException : MatchmakingException
{
    public Guid PlayerId { get; }
    public Guid MatchId { get; }

    public PlayerPresenceException(Guid playerId, Guid matchId, string message)
        : base($"Player {playerId} in match {matchId}: {message}")
    {
        PlayerId = playerId;
        MatchId = matchId;
    }
}