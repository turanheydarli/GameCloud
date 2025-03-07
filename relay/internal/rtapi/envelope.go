package rtapi

import (
	"context"

	"github.com/turanheydarli/gamecloud/relay/internal/session"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
	"google.golang.org/protobuf/proto"
)

func getOpCode(envelope *pbrt.Envelope) string {
	switch envelope.Message.(type) {
	// Authentication
	case *pbrt.Envelope_Connect:
		return "connect"
	case *pbrt.Envelope_Disconnect:
		return "disconnect"
	case *pbrt.Envelope_Heartbeat:
		return "heartbeat"

	// Player
	case *pbrt.Envelope_UpdatePlayer:
		return "update_player"
	case *pbrt.Envelope_UpdatePlayerAttributes:
		return "update_player_attributes"
	case *pbrt.Envelope_GetPlayerAttributes:
		return "get_player_attributes"
	case *pbrt.Envelope_DeletePlayerAttribute:
		return "delete_player_attribute"

	// Room
	case *pbrt.Envelope_RoomCreate:
		return "room_create"
	case *pbrt.Envelope_RoomJoin:
		return "room_join"
	case *pbrt.Envelope_RoomLeave:
		return "room_leave"
	case *pbrt.Envelope_RoomMessage:
		return "room_message"

	// RPC
	case *pbrt.Envelope_Rpc:
		return "rpc"
	case *pbrt.Envelope_RpcResult:
		return "rpc_result"
	case *pbrt.Envelope_RpcEvent:
		return "rpc_event"

	// Object Sync
	case *pbrt.Envelope_ObjectInstantiate:
		return "object_instantiate"
	case *pbrt.Envelope_ObjectSync:
		return "object_sync"
	case *pbrt.Envelope_ObjectSyncAck:
		return "object_sync_ack"
	case *pbrt.Envelope_ObjectDestroy:
		return "object_destroy"

	// Matchmaking
	case *pbrt.Envelope_MatchmakerAdd:
		return "matchmaker_add"
	case *pbrt.Envelope_MatchmakerRemove:
		return "matchmaker_remove"
	case *pbrt.Envelope_MatchmakerMatched:
		return "matchmaker_matched"

	// Other
	case *pbrt.Envelope_Error:
		return "error"
	default:
		return "unknown"
	}
}

func (h *Handler) processEnvelope(session *session.ClientSession, envelope *pbrt.Envelope) {
	ctx := context.WithValue(session.Ctx, "session", session)

	opCode := getOpCode(envelope)
	h.log.Infow("received message", "op_code", opCode, "session_id", session.ID)

	switch opCode {
	// Authentication
	case "connect":
		h.handleAuthentication(ctx, session, envelope)

	// Player
	case "update_player":
		h.handleUpdatePlayer(ctx, session, envelope)
	case "update_player_attributes":
		h.handleUpdatePlayerAttributes(ctx, session, envelope)
	case "get_player_attributes":
		h.handleGetPlayerAttributes(ctx, session, envelope)
	case "delete_player_attribute":
		h.handleDeletePlayerAttribute(ctx, session, envelope)

	// Room
	case "room_create":
		h.handleRoomCreate(ctx, session, envelope)
	case "room_join":
		h.handleRoomJoin(ctx, session, envelope)
	case "room_leave":
		h.handleRoomLeave(ctx, session, envelope)
	case "room_message":
		h.handleRoomMessage(ctx, session, envelope)

	// RPC
	case "rpc":
		h.handleRPC(ctx, session, envelope)

	// Object Sync
	case "object_instantiate":
		h.handleObjectInstantiate(ctx, session, envelope)
	case "object_sync":
		h.handleObjectSync(ctx, session, envelope)
	case "object_destroy":
		h.handleObjectDestroy(ctx, session, envelope)

	// Matchmaking
	case "matchmaker_add":
		h.handleMatchmakingCreateTicket(ctx, session, envelope)

	default:
		h.log.Warnw("unknown message type", "op_code", opCode)
		h.sendErrorToSession(session, envelope.Id, "unknown_message", "Unknown message type")
	}
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

func (h *Handler) sendEnvelopeToSession(session *session.ClientSession, envelope *pbrt.Envelope) {
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

func (h *Handler) sendErrorToSession(session *session.ClientSession, envelopeID, code, message string) {
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
