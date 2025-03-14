using System.Text.Json;
using GameCloud.Application.Features.Players;
using Grpc.Core;
using GameCloud.Proto;
using GameCloud.Application.Features.Games;
using Google.Protobuf.WellKnownTypes;
using Status = Grpc.Core.Status;
using GameCloud.Application.Features.Players.Requests;
using GameCloud.Application.Features.Rooms;

namespace GameCloud.WebAPI.Services.Grpc
{
    public sealed class RelayGrpcService : RelayService.RelayServiceBase
    {
        private readonly IRoomService _roomService;
        private readonly IPlayerService _playerService;
        private readonly IGameContext _gameContext;
        private readonly ILogger<RelayGrpcService> _logger;

        public RelayGrpcService(
            IPlayerService playerService,
            IGameContext gameContext,
            ILogger<RelayGrpcService> logger, IRoomService roomService)
        {
            _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
            _gameContext = gameContext ?? throw new ArgumentNullException(nameof(gameContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _roomService = roomService;
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

        #region Rooms

        public override async Task<Room> CreateRoom(CreateRoomRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Creating room for game ID: {GameId}", request.GameId);

            if (!Guid.TryParse(request.CreatorId, out var creatorId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid creator ID format"));
            }

            var roomConfig = new Application.Features.Rooms.Requests.RoomConfigRequest(
                request.Config.MinPlayers,
                request.Config.MaxPlayers,
                request.Config.TurnTimerSeconds,
                request.Config.AllowSpectators,
                request.Config.PersistState,
                request.Config.CustomConfig.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );

            var createRoomRequest = new Application.Features.Rooms.Requests.CreateRoomRequest(
                request.Label, // Using label as name
                request.GameId,
                creatorId,
                request.MaxPlayers,
                request.Private,
                request.Label,
                request.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                roomConfig
            );

            var roomResponse = await _roomService.CreateRoomAsync(createRoomRequest);

            var response = new Room
            {
                Id = roomResponse.Id.ToString(),
                GameId = roomResponse.GameId,
                CreatorId = roomResponse.CreatorId.ToString(),
                State = (RoomState)roomResponse.State,
                MaxPlayers = roomResponse.MaxPlayers,
                CurrentTurnUserId = roomResponse.CurrentTurnUserId ?? string.Empty,
                TurnNumber = roomResponse.TurnNumber,
                Config = new RoomConfig
                {
                    MinPlayers = roomResponse.Config.MinPlayers,
                    MaxPlayers = roomResponse.Config.MaxPlayers,
                    TurnTimerSeconds = roomResponse.Config.TurnTimerInSeconds,
                    AllowSpectators = roomResponse.Config.AllowSpectators,
                    PersistState = roomResponse.Config.PersistState
                }
            };

            foreach (var kvp in roomResponse.Metadata)
            {
                response.Metadata.Add(kvp.Key, kvp.Value);
            }

            foreach (var playerId in roomResponse.PlayerIds)
            {
                response.PlayerIds.Add(playerId);
            }

            foreach (var spectatorId in roomResponse.SpectatorIds)
            {
                response.SpectatorIds.Add(spectatorId);
            }

            foreach (var kvp in roomResponse.Config.CustomConfig)
            {
                response.Config.CustomConfig.Add(kvp.Key, kvp.Value);
            }

            response.CreatedAt =
                Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(roomResponse.CreatedAt.ToUniversalTime());

            return response;
        }

        public override async Task<Room> GetRoom(GetRoomRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Getting room: {RoomId}", request.RoomId);

            if (!Guid.TryParse(request.RoomId, out var roomId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid room ID format"));
            }

            var roomResponse = await _roomService.GetRoomAsync(roomId);

            var response = new Room
            {
                Id = roomResponse.Id.ToString(),
                GameId = roomResponse.GameId,
                CreatorId = roomResponse.CreatorId.ToString(),
                State = (RoomState)roomResponse.State,
                MaxPlayers = roomResponse.MaxPlayers,
                CurrentTurnUserId = roomResponse.CurrentTurnUserId ?? string.Empty,
                TurnNumber = roomResponse.TurnNumber,
                Config = new RoomConfig
                {
                    MinPlayers = roomResponse.Config.MinPlayers,
                    MaxPlayers = roomResponse.Config.MaxPlayers,
                    TurnTimerSeconds = roomResponse.Config.TurnTimerInSeconds,
                    AllowSpectators = roomResponse.Config.AllowSpectators,
                    PersistState = roomResponse.Config.PersistState
                }
            };

            foreach (var kvp in roomResponse.Metadata)
            {
                response.Metadata.Add(kvp.Key, kvp.Value);
            }

            foreach (var playerId in roomResponse.PlayerIds)
            {
                response.PlayerIds.Add(playerId);
            }

            foreach (var spectatorId in roomResponse.SpectatorIds)
            {
                response.SpectatorIds.Add(spectatorId);
            }

            foreach (var kvp in roomResponse.Config.CustomConfig)
            {
                response.Config.CustomConfig.Add(kvp.Key, kvp.Value);
            }

            response.CreatedAt =
                Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(roomResponse.CreatedAt.ToUniversalTime());

            return response;
        }

        public override async Task<Google.Protobuf.WellKnownTypes.Empty> DeleteRoom(DeleteRoomRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Deleting room: {RoomId}", request.RoomId);

            if (!Guid.TryParse(request.RoomId, out var roomId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid room ID format"));
            }

            var success = await _roomService.DeleteRoomAsync(roomId);
            if (!success)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Room not found"));
            }

            return new Google.Protobuf.WellKnownTypes.Empty();
        }

        public override async Task<Room> UpdateRoomState(UpdateRoomStateRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Updating room state: {RoomId}", request.RoomId);

            if (!Guid.TryParse(request.RoomId, out var roomId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid room ID format"));
            }

            var updateRequest = new Application.Features.Rooms.Requests.UpdateRoomStateRequest(
                roomId,
                (Domain.Entities.Rooms.RoomState)request.State,
                request.CurrentTurnUserId,
                request.TurnNumber,
                request.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );

            var roomResponse = await _roomService.UpdateRoomStateAsync(updateRequest);

            var response = new Room
            {
                Id = roomResponse.Id.ToString(),
                GameId = roomResponse.GameId,
                CreatorId = roomResponse.CreatorId.ToString(),
                State = (RoomState)roomResponse.State,
                MaxPlayers = roomResponse.MaxPlayers,
                CurrentTurnUserId = roomResponse.CurrentTurnUserId ?? string.Empty,
                TurnNumber = roomResponse.TurnNumber,
                Config = new RoomConfig
                {
                    MinPlayers = roomResponse.Config.MinPlayers,
                    MaxPlayers = roomResponse.Config.MaxPlayers,
                    TurnTimerSeconds = roomResponse.Config.TurnTimerInSeconds,
                    AllowSpectators = roomResponse.Config.AllowSpectators,
                    PersistState = roomResponse.Config.PersistState
                }
            };

            foreach (var kvp in roomResponse.Metadata)
            {
                response.Metadata.Add(kvp.Key, kvp.Value);
            }

            foreach (var playerId in roomResponse.PlayerIds)
            {
                response.PlayerIds.Add(playerId);
            }

            foreach (var spectatorId in roomResponse.SpectatorIds)
            {
                response.SpectatorIds.Add(spectatorId);
            }

            foreach (var kvp in roomResponse.Config.CustomConfig)
            {
                response.Config.CustomConfig.Add(kvp.Key, kvp.Value);
            }

            response.CreatedAt =
                Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(roomResponse.CreatedAt.ToUniversalTime());

            return response;
        }

        public override async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Player {UserId} joining room: {RoomId}", request.UserId, request.RoomId);

            if (!Guid.TryParse(request.RoomId, out var roomId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid room ID format"));
            }

            var joinRequest = new Application.Features.Rooms.Requests.JoinRoomRequest(
                roomId,
                request.UserId,
                request.SessionId,
                request.AsSpectator
            );

            var joinResponse = await _roomService.JoinRoomAsync(joinRequest);

            var response = new JoinRoomResponse
            {
                Success = joinResponse.Success,
                Error = joinResponse.Error ?? string.Empty,
                PlayerIndex = joinResponse.PlayerIndex
            };

            if (joinResponse.Room != null)
            {
                response.Room = new Room
                {
                    Id = joinResponse.Room.Id.ToString(),
                    GameId = joinResponse.Room.GameId,
                    CreatorId = joinResponse.Room.CreatorId.ToString(),
                    State = (RoomState)joinResponse.Room.State,
                    MaxPlayers = joinResponse.Room.MaxPlayers,
                    CurrentTurnUserId = joinResponse.Room.CurrentTurnUserId ?? string.Empty,
                    TurnNumber = joinResponse.Room.TurnNumber,
                    Config = new RoomConfig
                    {
                        MinPlayers = joinResponse.Room.Config.MinPlayers,
                        MaxPlayers = joinResponse.Room.Config.MaxPlayers,
                        TurnTimerSeconds = joinResponse.Room.Config.TurnTimerInSeconds,
                        AllowSpectators = joinResponse.Room.Config.AllowSpectators,
                        PersistState = joinResponse.Room.Config.PersistState
                    }
                };

                foreach (var kvp in joinResponse.Room.Metadata)
                {
                    response.Room.Metadata.Add(kvp.Key, kvp.Value);
                }

                foreach (var playerId in joinResponse.Room.PlayerIds)
                {
                    response.Room.PlayerIds.Add(playerId);
                }

                foreach (var spectatorId in joinResponse.Room.SpectatorIds)
                {
                    response.Room.SpectatorIds.Add(spectatorId);
                }

                foreach (var kvp in joinResponse.Room.Config.CustomConfig)
                {
                    response.Room.Config.CustomConfig.Add(kvp.Key, kvp.Value);
                }

                response.Room.CreatedAt =
                    Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(joinResponse.Room.CreatedAt
                        .ToUniversalTime());
            }

            return response;
        }

        public override async Task<Google.Protobuf.WellKnownTypes.Empty> LeaveRoom(LeaveRoomRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Player {UserId} leaving room: {RoomId}", request.UserId, request.RoomId);

            if (!Guid.TryParse(request.RoomId, out var roomId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid room ID format"));
            }

            var leaveRequest = new Application.Features.Rooms.Requests.LeaveRoomRequest(
                roomId,
                request.UserId,
                request.SessionId
            );

            var success = await _roomService.LeaveRoomAsync(leaveRequest);
            if (!success)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Room or player not found"));
            }

            return new Google.Protobuf.WellKnownTypes.Empty();
        }

        public override async Task<Google.Protobuf.WellKnownTypes.Empty> KickPlayer(KickPlayerRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Kicking player {UserId} from room: {RoomId}", request.UserId, request.RoomId);

            if (!Guid.TryParse(request.RoomId, out var roomId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid room ID format"));
            }

            var kickRequest = new Application.Features.Rooms.Requests.KickPlayerRequest(
                roomId,
                request.UserId,
                request.Reason
            );

            var success = await _roomService.KickPlayerAsync(kickRequest);
            if (!success)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Room or player not found"));
            }

            return new Google.Protobuf.WellKnownTypes.Empty();
        }

        public override async Task<Google.Protobuf.WellKnownTypes.Empty> PersistGameState(
            PersistGameStateRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Persisting game state for room: {RoomId}", request.RoomId);

            if (!Guid.TryParse(request.RoomId, out var roomId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid room ID format"));
            }

            var persistRequest = new Application.Features.Rooms.Requests.PersistGameStateRequest(
                roomId,
                request.StateData.ToByteArray(),
                request.CurrentTurnUserId,
                request.TurnNumber,
                request.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );

            var success = await _roomService.PersistGameStateAsync(persistRequest);
            if (!success)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Room not found"));
            }

            return new Google.Protobuf.WellKnownTypes.Empty();
        }

        #endregion
    }
}