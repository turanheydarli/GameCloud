package context

import (
	"context"
)

type contextKey string

const (
	ContextKeyGameKey contextKey = "game_key"

	ContextKeyPlayerID contextKey = "player_id"
)

func SetGameKey(ctx context.Context, gameKey string) context.Context {
	return context.WithValue(ctx, ContextKeyGameKey, gameKey)
}

func GetGameKey(ctx context.Context) (string, bool) {
	v, ok := ctx.Value(ContextKeyGameKey).(string)
	return v, ok
}

func SetPlayerID(ctx context.Context, playerID string) context.Context {
	return context.WithValue(ctx, ContextKeyPlayerID, playerID)
}

func GetPlayerID(ctx context.Context) (string, bool) {
	v, ok := ctx.Value(ContextKeyPlayerID).(string)
	return v, ok
}
