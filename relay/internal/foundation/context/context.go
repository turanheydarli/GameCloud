package context

import (
	"context"
)

type contextKey string

const (
	GameKeyContextKey  contextKey = "game_key"
	PlayerIDContextKey contextKey = "player_id"
	SessionContextKey  contextKey = "session"
)

func SetGameKey(ctx context.Context, gameKey string) context.Context {
	return context.WithValue(ctx, GameKeyContextKey, gameKey)
}

func GetGameKey(ctx context.Context) (string, bool) {
	v, ok := ctx.Value(GameKeyContextKey).(string)
	return v, ok
}

func SetPlayerID(ctx context.Context, playerID string) context.Context {
	return context.WithValue(ctx, PlayerIDContextKey, playerID)
}

func GetPlayerID(ctx context.Context) (string, bool) {
	v, ok := ctx.Value(PlayerIDContextKey).(string)
	return v, ok
}

func WithSession(ctx context.Context, session interface{}) context.Context {
	return context.WithValue(ctx, SessionContextKey, session)
}

func FromContext(ctx context.Context) (interface{}, bool) {
	session := ctx.Value(SessionContextKey)
	return session, session != nil
}
