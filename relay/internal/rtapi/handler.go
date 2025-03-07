package rtapi

import (
	"context"
	"errors"
	"net/http"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/gorilla/websocket"
	"github.com/turanheydarli/gamecloud/relay/internal/player"
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/metadata"
	"google.golang.org/grpc/status"
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
		GameKey:   "",
		Send:      make(chan []byte, 256),
		Ctx:       ctx,
		Cancel:    cancel,
		CreatedAt: time.Now(),
	}

	gameKey := r.Header.Get("X-Game-Key")
	if gameKey == "" {
		h.log.Errorw("game key not found", "session_id", session.ID)
		h.sendErrorToSession(session, "", "invalid_payload", "Game key not found")
		return
	}

	session.GameKey = gameKey

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

	if session.GameKey == "" {
		h.log.Errorw("game key not found in session", "session_id", session.ID)
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Game key not found")
		return
	}

	h.log.Infow("authenticating with game key", "game_key", session.GameKey, "session_id", session.ID)

	grpcCtx := h.createGRPCContext(ctx, session.GameKey)

	authReq := &pbrt.AuthenticateRequest{
		DeviceId: deviceId,
	}

	authResp, err := h.playerService.Authenticate(grpcCtx, authReq)
	if err != nil {
		h.handleGRPCError(session, envelope.Id, err)
		return
	}

	session.PlayerID = authResp.PlayerId
	jwtToken := authResp.Token

	h.clientsMu.Lock()
	h.clients[authResp.PlayerId] = session
	h.clientsMu.Unlock()

	h.log.Infow("player authenticated",
		"session_id", session.ID,
		"player_id", authResp.PlayerId,
		"game_key", session.GameKey)

	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_Connect{
			Connect: &pbrt.SessionConnect{
				Token: jwtToken,
				ConnectionMetadata: map[string]string{
					"user_id": authResp.PlayerId,
					"status":  "online",
				},
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

func (h *Handler) closeSession(session *ClientSession) {
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

func MarshalProto(envelope *pbrt.Envelope) ([]byte, error) {
	return proto.Marshal(envelope)
}

func UnmarshalProto(data []byte) (*pbrt.Envelope, error) {
	envelope := &pbrt.Envelope{}
	if err := proto.Unmarshal(data, envelope); err != nil {
		return nil, err
	}
	return envelope, nil
}

type contextKey string

const (
	contextKeyGameKey  contextKey = "game_key"
	contextKeyPlayerID contextKey = "player_id"
)

func generateSessionID() string {
	return uuid.New().String()
}

func (h *Handler) createGRPCContext(ctx context.Context, gameKey string) context.Context {
	if gameKey == "" {
		h.log.Warnw("no game key available for gRPC call")
		return ctx
	}

	md := metadata.Pairs("X-Game-Key", gameKey)
	return metadata.NewOutgoingContext(ctx, md)
}

func (h *Handler) handleGRPCError(session *ClientSession, envelopeID string, err error) {
	st, ok := status.FromError(err)
	if !ok {
		h.log.Errorw("non-gRPC error occurred", "error", err, "session_id", session.ID)
		h.sendErrorToSession(session, envelopeID, "internal_error", "An internal error occurred")
		return
	}

	var code string
	var message string

	switch st.Code() {
	case codes.Unauthenticated:
		code = "authentication_failed"
		message = st.Message()
	case codes.PermissionDenied:
		code = "permission_denied"
		message = st.Message()
	case codes.InvalidArgument:
		code = "invalid_argument"
		message = st.Message()
	case codes.NotFound:
		code = "not_found"
		message = st.Message()
	case codes.AlreadyExists:
		code = "already_exists"
		message = st.Message()
	case codes.ResourceExhausted:
		code = "resource_exhausted"
		message = st.Message()
	case codes.FailedPrecondition:
		code = "failed_precondition"
		message = st.Message()
	case codes.Aborted:
		code = "aborted"
		message = st.Message()
	case codes.DeadlineExceeded:
		code = "timeout"
		message = "The operation timed out"
	case codes.Unavailable:
		code = "service_unavailable"
		message = "The service is currently unavailable"
	default:
		code = "internal_error"
		message = "An internal error occurred"
	}

	h.log.Errorw("gRPC error occurred",
		"error", err,
		"grpc_code", st.Code(),
		"grpc_message", st.Message(),
		"client_code", code,
		"session_id", session.ID)

	var details []string
	for _, detail := range st.Details() {
		if d, ok := detail.(proto.Message); ok {
			detailBytes, err := proto.Marshal(d)
			if err == nil {
				details = append(details, string(detailBytes))
			}
		}
	}

	h.sendErrorToSession(session, envelopeID, code, message)
}
