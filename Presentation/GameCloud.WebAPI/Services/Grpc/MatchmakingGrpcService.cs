using Grpc.Core;
using GameCloud.Proto;
using GameCloud.Application.Features.Matchmakers;

namespace GameCloud.WebAPI.Services.Grpc;

public class MatchmakingGrpcService(ILogger<MatchmakingGrpcService> logger, IMatchmakingService matchmakingService)
    : MatchmakingService.MatchmakingServiceBase
{
    public override async Task<MatchmakingTicket> CreateTicket(CreateTicketRequest request, ServerCallContext context)
    {
        logger.LogDebug("Received CreateTicket from .NET gRPC, gameId={GameId}, playerId={PlayerId}", request.GameId,
            request.PalyerId);

        var gameId = Guid.Parse(request.GameId);
        var playerId = Guid.Parse(request.PalyerId);

        var ticket = await matchmakingService.CreateTicketAsync(
            gameId,
            playerId,
            request.QueueName
        );

        return new MatchmakingTicket
        {
            TicketId = ticket.Id.ToString()
        };
    }
}