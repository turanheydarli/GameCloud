package rtapi

import (
	"context"
	"encoding/json"
	"net/http"
	"sync"
	"time"

	"github.com/gorilla/websocket"
	"github.com/turanheydarli/gamecloud/relay/internal/player"
	"github.com/turanheydarli/gamecloud/relay/internal/room"
	"github.com/turanheydarli/gamecloud/relay/internal/rpc"
	"github.com/turanheydarli/gamecloud/relay/internal/session"
	gameSync "github.com/turanheydarli/gamecloud/relay/internal/sync"
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
)

type Handler struct {
	log           logger.Logger
	playerService *player.Service
	roomService   *room.Service
	syncService   *gameSync.Service
	rpcService    *rpc.Service
	clients       map[string]*session.ClientSession
	clientsMu     sync.RWMutex
	upgrader      websocket.Upgrader
}

func NewHandler(
	log logger.Logger,
	playerService *player.Service,
	syncService *gameSync.Service,
	rpcService *rpc.Service,
	roomService *room.Service,
) *Handler {
	upgrader := websocket.Upgrader{
		ReadBufferSize:  1024,
		WriteBufferSize: 1024,
		CheckOrigin: func(r *http.Request) bool {
			return true
		},
	}

	h := &Handler{
		log:           log,
		playerService: playerService,
		roomService:   roomService,
		syncService:   syncService,
		rpcService:    rpcService,
		clients:       make(map[string]*session.ClientSession),
		upgrader:      upgrader,
	}

	// Register default RPC handlers
	h.registerDefaultRPCHandlers()

	return h
}

func (h *Handler) registerDefaultRPCHandlers() {
	// Echo function - returns the same data it receives
	h.rpcService.RegisterServerFunction("echo", func(ctx context.Context, playerID string, params []byte) ([]byte, error) {
		return params, nil
	})

	// GetServerTime function - returns the current server time
	h.rpcService.RegisterServerFunction("getServerTime", func(ctx context.Context, playerID string, params []byte) ([]byte, error) {
		timeData, err := json.Marshal(map[string]interface{}{
			"timestamp": time.Now().Unix(),
		})
		return timeData, err
	})
}

func (h *Handler) HandleWebSocket(w http.ResponseWriter, r *http.Request) {
	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		h.log.Errorw("failed to upgrade connection", "error", err)
		return
	}

	cSession := session.New(conn, "")

	gameKey := r.Header.Get("X-Game-Key")
	if gameKey == "" {
		h.log.Errorw("game key not found", "session_id", cSession.ID)
		h.sendErrorToSession(cSession, "", "invalid_payload", "Game key not found")
		return
	}

	cSession.GameKey = gameKey

	h.clientsMu.Lock()
	h.clients[cSession.ID] = cSession
	h.clientsMu.Unlock()

	h.log.Infow("new websocket connection", "session_id", cSession.ID)

	go h.readPump(cSession)
	go h.writePump(cSession)
}

func (h *Handler) readPump(session *session.ClientSession) {
	defer func() {
		h.closeSession(session)
	}()

	session.Conn.SetReadLimit(512 * 1024)
	session.Conn.SetReadDeadline(time.Now().Add(60 * time.Second))
	session.Conn.SetPongHandler(func(string) error {
		session.Conn.SetReadDeadline(time.Now().Add(60 * time.Second))
		return nil
	})

	for {
		_, message, err := session.Conn.ReadMessage()
		if err != nil {
			if websocket.IsUnexpectedCloseError(err, websocket.CloseGoingAway, websocket.CloseAbnormalClosure) {
				h.log.Warnw("websocket unexpected close", "session_id", session.ID, "error", err)
			}
			break
		}

		envelope, err := UnmarshalProto(message)
		if err != nil {
			h.log.Errorw("failed to unmarshal message", "session_id", session.ID, "error", err)
			h.sendErrorToSession(session, "", "invalid_payload", "Invalid message format")
			continue
		}

		h.processEnvelope(session, envelope)
	}
}

func (h *Handler) writePump(session *session.ClientSession) {
	ticker := time.NewTicker(30 * time.Second)
	defer func() {
		ticker.Stop()
	}()

	for {
		select {
		case message, ok := <-session.Send:
			session.Conn.SetWriteDeadline(time.Now().Add(10 * time.Second))
			if !ok {
				session.Conn.WriteMessage(websocket.CloseMessage, []byte{})
				return
			}

			w, err := session.Conn.NextWriter(websocket.BinaryMessage)
			if err != nil {
				h.log.Errorw("error getting writer", "session_id", session.ID, "error", err)
				return
			}

			_, err = w.Write(message)
			if err != nil {
				h.log.Errorw("error writing message", "session_id", session.ID, "error", err)
			}

			if err := w.Close(); err != nil {
				h.log.Errorw("error closing writer", "session_id", session.ID, "error", err)
				return
			}

		case <-ticker.C:
			session.Conn.SetWriteDeadline(time.Now().Add(10 * time.Second))
			if err := session.Conn.WriteMessage(websocket.PingMessage, nil); err != nil {
				return
			}

		case <-session.Ctx.Done():
			return
		}
	}
}

func (h *Handler) SendToPlayer(playerID string, envelope *pbrt.Envelope) error {
	h.clientsMu.RLock()
	session, exists := h.clients[playerID]
	h.clientsMu.RUnlock()

	if !exists {
		return ErrPlayerNotConnected
	}

	h.sendEnvelopeToSession(session, envelope)
	return nil
}

func (h *Handler) closeSession(session *session.ClientSession) {
	h.clientsMu.Lock()
	defer h.clientsMu.Unlock()

	if _, exists := h.clients[session.ID]; !exists {
		return
	}

	delete(h.clients, session.ID)
	if session.PlayerID != "" {
		delete(h.clients, session.PlayerID)
	}

	session.Cancel()
	close(session.Send)
	session.Conn.Close()

	h.log.Infow("closed websocket session", "session_id", session.ID, "player_id", session.PlayerID)
}
