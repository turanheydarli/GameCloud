package session

import (
	"context"
	"time"

	"github.com/gorilla/websocket"
)

// ClientSession represents a connected client
type ClientSession struct {
	ID        string
	Conn      *websocket.Conn
	PlayerID  string
	GameKey   string
	Send      chan []byte
	Ctx       context.Context
	Cancel    context.CancelFunc
	CreatedAt time.Time
}

// New creates a new client session
func New(conn *websocket.Conn, gameKey string) *ClientSession {
	ctx, cancel := context.WithCancel(context.Background())
	return &ClientSession{
		ID:        GenerateID(),
		Conn:      conn,
		GameKey:   gameKey,
		Send:      make(chan []byte, 256),
		Ctx:       ctx,
		Cancel:    cancel,
		CreatedAt: time.Now(),
	}
}
