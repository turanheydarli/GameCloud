package rtapi

import (
	"context"
	"encoding/binary"
	"fmt"
	"net/http"

	"github.com/gorilla/websocket"
	"github.com/turanheydarli/gamecloud/relay/internal/matchmaking"
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	pbrt "github.com/turanheydarli/gamecloud/proto"
)

type Handler struct {
	log       logger.Logger
	wsManager *WebSocketManager
	mmService *matchmaking.Service
	upgrader  websocket.Upgrader
}

func NewHandler(
	log logger.Logger,
	wsManager *WebSocketManager,
	mmService *matchmaking.Service,
) *Handler {
	return &Handler{
		log:       log,
		wsManager: wsManager,
		mmService: mmService,
		upgrader: websocket.Upgrader{
			CheckOrigin: func(r *http.Request) bool { return true },
		},
	}
}

func (h *Handler) HandleWebSocket(w http.ResponseWriter, r *http.Request) {
	playerID := r.URL.Query().Get("player_id")
	if playerID == "" {
		http.Error(w, "Missing player_id", http.StatusBadRequest)
		return
	}

	conn, err := h.upgrader.Upgrade(w, r, nil)
	if err != nil {
		h.log.Errorw("WebSocket upgrade error", "error", err)
		return
	}

	h.wsManager.RegisterConnection(playerID, conn)

	go h.readLoop(playerID, conn)
}

func (h *Handler) readLoop(playerID string, conn *websocket.Conn) {
	defer func() {
		conn.Close()
		h.wsManager.UnregisterConnection(playerID)
	}()

	for {
		msgType, data, err := conn.ReadMessage()
		if err != nil {
			h.log.Warnw("readLoop: connection closed", "playerID", playerID, "error", err)
			return
		}

		if msgType != websocket.BinaryMessage {
			h.log.Warnw("Dropping non-binary message", "playerID", playerID)
			continue
		}

		envelope, err := UnmarshalProto(data)
		if err != nil {
			h.log.Errorw("readLoop: unmarshal error", "error", err)
			continue
		}
		h.processEnvelope(playerID, envelope)
	}
}

func (h *Handler) processEnvelope(playerID string, envelope *pbrt.Envelope) {
	switch msg := envelope.Message.(type) {

	case *pbrt.Envelope_MatchmakerAdd:
		h.log.Infow("Received MatchmakerAdd", "player", playerID, "queue", msg.MatchmakerAdd.GameId)
		// Here, create a ticket
		ticketID, err := h.mmService.CreateTicket(
			h.makeContext(), 
			playerID, 
			msg.MatchmakerAdd.GameId,
		)
		if err != nil {
			h.sendError(playerID, envelope.Id, "MATCHMAKER_ADD_FAILED", err.Error())
			return
		}

		ack := &pbrt.Envelope{
			Id: envelope.Id,
			Message: &pbrt.Envelope_Status{
				Status: &pbrt.Status{
					PresenceStatuses: map[string]string{
						"ticket_created": ticketID,
					},
				},
			},
		}
		h.wsManager.SendToPlayer(playerID, ack)

	case *pbrt.Envelope_MatchmakerRemove:
		h.log.Infow("MatchmakerRemove", "player", playerID, "ticket", msg.MatchmakerRemove.Ticket)

	default:
		h.log.Warnw("Unknown envelope type", "type", fmt.Sprintf("%T", envelope.Message))
		h.sendError(playerID, envelope.Id, "UNKNOWN_TYPE", "Unrecognized message type")
	}
}

func (h *Handler) makeContext() context.Context {
	return context.Background()
}

func (h *Handler) sendError(playerID, envelopeID, code, msg string) {
	errEnv := &pbrt.Envelope{
		Id: envelopeID,
		Message: &pbrt.Envelope_Error{
			Error: &pbrt.Error{
				Code:    code,
				Message: msg,
			},
		},
	}
	h.wsManager.SendToPlayer(playerID, errEnv)
} 