using GameCloud.Application.Features.Games;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GameCloud.WebAPI.Middlewares
{
    public class GameContextInterceptor : Interceptor
    {
        private readonly ILogger<GameContextInterceptor> _logger;
        private readonly IGameKeyResolver _gameKeyResolver;

        public GameContextInterceptor(
            ILogger<GameContextInterceptor> logger,
            IGameKeyResolver gameKeyResolver)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameKeyResolver = gameKeyResolver ?? throw new ArgumentNullException(nameof(gameKeyResolver));
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var gameKey = context.RequestHeaders.GetValue("X-Game-Key");

            if (string.IsNullOrEmpty(gameKey))
            {
                gameKey = context.RequestHeaders.GetValue("game-key");
            }

            if (string.IsNullOrEmpty(gameKey))
            {
                var httpContext = context.GetHttpContext();
                if (httpContext.Request.Query.ContainsKey("game_key"))
                {
                    gameKey = httpContext.Request.Query["game_key"];
                }
            }

            if (!string.IsNullOrEmpty(gameKey))
            {
                var gameId = await _gameKeyResolver.ResolveGameIdAsync(gameKey);
                if (gameId != Guid.Empty)
                {
                    GameContext gameContext = new GameContext(gameId);

                    _logger.LogDebug("Game context set with ID: {GameId} from key: {GameKey}", gameId, gameKey);

                    var httpContext = context.GetHttpContext();

                    httpContext.Items["GameContext"] = gameContext;
                }
                else
                {
                    _logger.LogWarning("Invalid game key: {GameKey}", gameKey);
                }
            }
            else
            {
                _logger.LogWarning("No game key found in request");
            }

            return await continuation(request, context);
        }
    }
}