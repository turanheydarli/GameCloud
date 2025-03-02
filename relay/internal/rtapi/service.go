package rtapi

import (
	"sync"

	"github.com/gorilla/websocket"
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
	"google.golang.org/protobuf/proto"
)

type WebSocketManager struct {
	log         logger.Logger
	connections map[string]*websocket.Conn
	mu          sync.RWMutex
}

func NewWebSocketManager(log logger.Logger) *WebSocketManager {
	return &WebSocketManager{
		log:         log,
		connections: make(map[string]*websocket.Conn),
	}
}

func (m *WebSocketManager) RegisterConnection(playerID string, conn *websocket.Conn) {
	m.mu.Lock()
	defer m.mu.Unlock()
	m.connections[playerID] = conn
	m.log.Infow("RegisterConnection", "playerID", playerID)
}

func (m *WebSocketManager) UnregisterConnection(playerID string) {
	m.mu.Lock()
	defer m.mu.Unlock()
	delete(m.connections, playerID)
	m.log.Infow("UnregisterConnection", "playerID", playerID)
}

func (m *WebSocketManager) SendToPlayer(playerID string, msg *pbrt.Envelope) {
	m.mu.RLock()
	conn, ok := m.connections[playerID]
	m.mu.RUnlock()
	if !ok {
		m.log.Warnw("SendToPlayer: no connection found", "playerID", playerID)
		return
	}

	data, err := MarshalProto(msg)
	if err != nil {
		m.log.Errorw("SendToPlayer: marshal error", "error", err)
		return
	}

	if err := conn.WriteMessage(websocket.BinaryMessage, data); err != nil {
		m.log.Errorw("SendToPlayer: write error", "error", err)
	}
}

func MarshalProto(msg *pbrt.Envelope) ([]byte, error) {
	return proto.Marshal(msg)
}
