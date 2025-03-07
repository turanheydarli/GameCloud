package session

import (
	"context"
	"errors"

	"github.com/google/uuid"
)

var ErrPlayerNotConnected = errors.New("player not connected")

type contextKey string

const (
	SessionContextKey  contextKey = "session"
	GameKeyContextKey  contextKey = "game_key"
	PlayerIDContextKey contextKey = "player_id"
)

func GenerateID() string {
	return uuid.New().String()
}

func FromContext(ctx context.Context) (*ClientSession, bool) {
	session, ok := ctx.Value("session").(*ClientSession)
	return session, ok
}

func WithSession(ctx context.Context, session *ClientSession) context.Context {
	return context.WithValue(ctx, SessionContextKey, session)
}

func GetGameKey(ctx context.Context) (string, bool) {
	v, ok := ctx.Value(GameKeyContextKey).(string)
	return v, ok
}

func GetPlayerID(ctx context.Context) (string, bool) {
	v, ok := ctx.Value(PlayerIDContextKey).(string)
	return v, ok
}
