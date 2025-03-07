using System.Text.Json;
using GameCloud.Application.Features.Players;
using Grpc.Core;
using GameCloud.Proto;
using GameCloud.Application.Features.Games;
using Google.Protobuf.WellKnownTypes;
using Status = Grpc.Core.Status;
using GameCloud.Application.Features.Players.Requests;

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
                Token = request.DeviceId
            };

            response.ExpiresAt = Timestamp.FromDateTime(authResult.ExpiresAt);

            return response;
        }

        public override async Task<UpdatePlayerResponse> UpdatePlayer(UpdatePlayerRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Updating player {PlayerId} for game ID: {GameId}",
                request.PlayerId, _gameContext.GameId);

            if (!Guid.TryParse(request.PlayerId, out var playerId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid player ID format"));
            }

            Dictionary<string, object> meta = new Dictionary<string, object>();

            if (request.Metadata is { Count: > 0 })
            {
                meta = request.Metadata.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (object)kvp.Value);
            }

            var playerRequest = new PlayerRequest(Guid.Empty, "", request.DisplayName, meta);

            var updatedPlayer = await _playerService.UpdateAsync(playerId, playerRequest);

            var response = new UpdatePlayerResponse
            {
                PlayerId = updatedPlayer.Id.ToString(),
                DisplayName = updatedPlayer.DisplayName ?? string.Empty,
            };

            if (updatedPlayer.Metadata != null)
            {
                foreach (var kvp in updatedPlayer.Metadata.Deserialize<Dictionary<string, object>>()!)
                {
                    response.Metadata[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                }
            }

            return response;
        }

        public override async Task<UpdatePlayerAttributesResponse> UpdatePlayerAttributes(
            UpdatePlayerAttributesRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Updating player {PlayerId} attributes for game ID: {GameId}",
                request.PlayerId, _gameContext.GameId);

            if (!Guid.TryParse(request.PlayerId, out var playerId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid player ID format"));
            }

            await _playerService.SetAttributeAsync(
                playerId.ToString(),
                request.Collection, new AttributeRequest(request.Key, request.Value, null, null, null, null));

            var response = new UpdatePlayerAttributesResponse
            {
                PlayerId = playerId.ToString(),
                Collection = request.Collection,
                Key = request.Key,
                Value = request.Value
            };

            return response;
        }

        public override async Task<GetPlayerAttributesResponse> GetPlayerAttributes(GetPlayerAttributesRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Getting player {PlayerId} attributes for game ID: {GameId}", request.PlayerId,
                _gameContext.GameId);

            if (!Guid.TryParse(request.PlayerId, out var playerId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid player ID format"));
            }

            var response = new GetPlayerAttributesResponse
            {
                PlayerId = playerId.ToString(),
                Collection = request.Collection,
                Key = request.Key
            };

            if (!string.IsNullOrEmpty(request.Key))
            {
                var attribute = await _playerService.GetAttributeAsync(
                    playerId.ToString(),
                    request.Collection,
                    request.Key);

                response.Value = attribute.Value;
            }
            else
            {
                var attributes = await _playerService.GetAttributesAsync(
                    request.Collection,
                    playerId.ToString());

                foreach (var attr in attributes.Collections)
                {
                    response.Attributes[attr.Key] = JsonSerializer.Serialize(attr.Value);
                }
            }

            return response;
        }

        public override async Task<DeletePlayerAttributeResponse> DeletePlayerAttribute(
            DeletePlayerAttributeRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Deleting player {PlayerId} attribute for game ID: {GameId}",
                request.PlayerId, _gameContext.GameId);

            if (!Guid.TryParse(request.PlayerId, out var playerId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid player ID format"));
            }

            await _playerService.RemoveAttributeAsync(
                playerId.ToString(),
                request.Collection,
                request.Key);

            var response = new DeletePlayerAttributeResponse
            {
                PlayerId = playerId.ToString(),
                Collection = request.Collection,
                Key = request.Key,
                Success = true
            };

            return response;
        }
    }
}