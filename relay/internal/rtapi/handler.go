package rtapi

import (
	"context"
	"errors"
	"net/http"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/gorilla/websocket"
	gameCtx "github.com/turanheydarli/gamecloud/relay/internal/foundation/context"
	"github.com/turanheydarli/gamecloud/relay/internal/player"
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
	"google.golang.org/protobuf/proto"
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

type Handler struct {
	log           logger.Logger
	playerService *player.Service
	clients       map[string]*ClientSession
	clientsMu     sync.RWMutex
	upgrader      websocket.Upgrader
}

func NewHandler(
	log logger.Logger,
	playerService *player.Service,
) *Handler {
	upgrader := websocket.Upgrader{
		ReadBufferSize:  1024,
		WriteBufferSize: 1024,
		CheckOrigin: func(r *http.Request) bool {
			return true
		},
	}

	return &Handler{
		log:           log,
		playerService: playerService,
		clients:       make(map[string]*ClientSession),
		upgrader:      upgrader,
	}
}

func (h *Handler) HandleWebSocket(w http.ResponseWriter, r *http.Request) {
	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		h.log.Errorw("failed to upgrade connection", "error", err)
		return
	}

	ctx, cancel := context.WithCancel(context.Background())
	session := &ClientSession{
		ID:        generateSessionID(),
		Conn:      conn,
		PlayerID:  "", // Will be set after authentication
		GameKey:   "", // Will be set after authentication
		Send:      make(chan []byte, 256),
		Ctx:       ctx,
		Cancel:    cancel,
		CreatedAt: time.Now(),
	}

	h.clientsMu.Lock()
	h.clients[session.ID] = session
	h.clientsMu.Unlock()

	h.log.Infow("new websocket connection", "session_id", session.ID)

	go h.readPump(session)
	go h.writePump(session)
}

func (h *Handler) readPump(session *ClientSession) {
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

func (h *Handler) writePump(session *ClientSession) {
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

func getOpCode(envelope *pbrt.Envelope) string {
	switch envelope.Message.(type) {
	case *pbrt.Envelope_Connect:
		return "connect"
	case *pbrt.Envelope_Disconnect:
		return "disconnect"
	case *pbrt.Envelope_Heartbeat:
		return "heartbeat"
	case *pbrt.Envelope_RoomCreate:
		return "room_create"
	case *pbrt.Envelope_RoomJoin:
		return "room_join"
	case *pbrt.Envelope_RoomLeave:
		return "room_leave"
	case *pbrt.Envelope_RoomData:
		return "room_data"
	case *pbrt.Envelope_RoomPresence:
		return "room_presence"
	case *pbrt.Envelope_MatchmakerAdd:
		return "matchmaker_add"
	case *pbrt.Envelope_MatchmakerRemove:
		return "matchmaker_remove"
	case *pbrt.Envelope_MatchmakerMatched:
		return "matchmaker_matched"
	case *pbrt.Envelope_Status:
		return "status"
	case *pbrt.Envelope_StatusUpdate:
		return "status_update"
	case *pbrt.Envelope_StatusPresence:
		return "status_presence"
	case *pbrt.Envelope_GameState:
		return "game_state"
	case *pbrt.Envelope_GameAction:
		return "game_action"
	case *pbrt.Envelope_GameActionAck:
		return "game_action_ack"
	case *pbrt.Envelope_TurnChange:
		return "turn_change"
	case *pbrt.Envelope_GameEnd:
		return "game_end"
	case *pbrt.Envelope_Error:
		return "error"
	default:
		return "unknown"
	}
}

const sessionContextKey = contextKey("session")

func (h *Handler) processEnvelope(session *ClientSession, envelope *pbrt.Envelope) {
	ctx := context.WithValue(session.Ctx, sessionContextKey, session)

	opCode := getOpCode(envelope)
	h.log.Infow("received message", "op_code", opCode, "session_id", session.ID)

	switch opCode {
	case "connect":
		h.handleAuthentication(ctx, session, envelope)
	case "validate_token":
		h.handleTokenValidation(ctx, session, envelope)
	case "matchmaker_add":
		h.handleMatchmakingCreateTicket(ctx, session, envelope)
	default:
		h.log.Warnw("unknown message type", "op_code", opCode)
		h.sendErrorToSession(session, envelope.Id, "unknown_message", "Unknown message type")
	}
}

func (h *Handler) handleAuthentication(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope) {
	authMsg, ok := envelope.Message.(*pbrt.Envelope_Connect)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid authentication payload")
		return
	}

	connectMsg := authMsg.Connect

	deviceId := connectMsg.DeviceId

	gameKey := ""
	if metadata, ok := connectMsg.ConnectionMetadata["game_key"]; ok {
		gameKey = metadata
	}

	session.GameKey = gameKey

	authReq := &pbrt.AuthenticateRequest{
		DeviceId: deviceId,
	}

	authResp, err := h.playerService.Authenticate(ctx, authReq)
	if err != nil {
		h.log.Errorw("authentication failed", "error", err, "session_id", session.ID)
		h.sendErrorToSession(session, envelope.Id, "authentication_failed", "Failed to authenticate user")
		return
	}

	session.PlayerID = authResp.UserId

	ctx = gameCtx.SetGameKey(ctx, gameKey)
	ctx = gameCtx.SetPlayerID(ctx, authResp.UserId)

	h.clientsMu.Lock()
	h.clients[authResp.UserId] = session
	h.clientsMu.Unlock()

	h.log.Infow("player authenticated",
		"session_id", session.ID,
		"player_id", authResp.UserId,
		"game_key", gameKey)

	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_Connect{
			Connect: &pbrt.SessionConnect{
				Token: authResp.SessionToken,
				ConnectionMetadata: map[string]string{
					"user_id": authResp.UserId,
					"status":  "online",
				},
			},
		},
	}

	h.sendEnvelopeToSession(session, response)
}

func (h *Handler) handleTokenValidation(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope) {
	// This is a placeholder implementation
	// You'll need to adapt this to your actual token validation logic

	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_Status{
			Status: &pbrt.Status{
				PresenceStatuses: map[string]string{"status": "validated"},
			},
		},
	}

	h.sendEnvelopeToSession(session, response)
}

func (h *Handler) handleMatchmakingCreateTicket(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope) {
}

func (h *Handler) sendEnvelopeToSession(session *ClientSession, envelope *pbrt.Envelope) {
	data, err := MarshalProto(envelope)
	if err != nil {
		h.log.Errorw("failed to marshal envelope", "error", err)
		return
	}

	select {
	case session.Send <- data:
		// Message sent
	default:
		// Buffer full, close session
		h.closeSession(session)
	}
}

func (h *Handler) sendErrorToSession(session *ClientSession, envelopeID, code, message string) {
	// Create error response using the Error message type
	response := &pbrt.Envelope{
		Id: envelopeID,
		Message: &pbrt.Envelope_Error{
			Error: &pbrt.Error{
				Code:    code,
				Message: message,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)
}

// SendToPlayer sends a message to a specific player by ID
func (h *Handler) SendToPlayer(playerID string, envelope *pbrt.Envelope) error {
	h.clientsMu.RLock()
	session, exists := h.clients[playerID]
	h.clientsMu.RUnlock()

	if !exists {
		return errors.New("player not connected")
	}

	h.sendEnvelopeToSession(session, envelope)
	return nil
}

// closeSession closes a client session and cleans up resources
func (h *Handler) closeSession(session *ClientSession) {
	h.clientsMu.Lock()
	defer h.clientsMu.Unlock()

	// Only process if not already closed
	if _, exists := h.clients[session.ID]; !exists {
		return
	}

	// Remove by session ID and player ID if authenticated
	delete(h.clients, session.ID)
	if session.PlayerID != "" {
		delete(h.clients, session.PlayerID)
	}

	// Cancel the context to terminate goroutines
	session.Cancel()

	// Close the send channel
	close(session.Send)

	// Close the WebSocket connection
	session.Conn.Close()

	h.log.Infow("closed websocket session", "session_id", session.ID, "player_id", session.PlayerID)
}

// MarshalProto marshals a protobuf envelope to bytes
func MarshalProto(envelope *pbrt.Envelope) ([]byte, error) {
	return proto.Marshal(envelope)
}

// UnmarshalProto unmarshals bytes to a protobuf envelope
func UnmarshalProto(data []byte) (*pbrt.Envelope, error) {
	envelope := &pbrt.Envelope{}
	if err := proto.Unmarshal(data, envelope); err != nil {
		return nil, err
	}
	return envelope, nil
}

// Helper context keys
type contextKey string

const (
	contextKeyGameKey  contextKey = "game_key"
	contextKeyPlayerID contextKey = "player_id"
)

// generateSessionID generates a unique session ID
func generateSessionID() string {
	return uuid.New().String()
}
