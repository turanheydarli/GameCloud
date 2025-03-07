package rtapi

import (
	"context"
	"time"

	"github.com/gorilla/websocket"
)

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
