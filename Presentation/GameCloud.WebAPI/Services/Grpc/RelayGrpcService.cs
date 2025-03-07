using GameCloud.Application.Features.Players;
using Grpc.Core;
using GameCloud.Proto;
using GameCloud.Application.Features.Games;
using Status = Grpc.Core.Status;

namespace GameCloud.WebAPI.Services.Grpc
{
    public sealed class RelayGrpcService : RelayService.RelayServiceBase
    {
        private readonly IPlayerService _playerService;
        private readonly IGameContext _gameContext;
        private readonly ILogger<RelayGrpcService> _logger;

        public RelayGrpcService(
            IPlayerService playerService,
            IGameContext gameContext,
            ILogger<RelayGrpcService> logger)
        {
            _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
            _gameContext = gameContext ?? throw new ArgumentNullException(nameof(gameContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<AuthenticateResponse> AuthenticateUser(AuthenticateRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Processing authentication for game ID: {GameId}", _gameContext.GameId);

            var authResult = await _playerService.AuthenticateWithDeviceAsync(request.DeviceId);

            var response = new AuthenticateResponse
            {
                PlayerId = authResult.Player.Id.ToString(),
                Created = false,
                Token = authResult.Token,
                ExpiresAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(authResult.ExpiresAt),
                RefreshToken = authResult.RefreshToken ?? "",
            };

            return response;
        }
    }
}