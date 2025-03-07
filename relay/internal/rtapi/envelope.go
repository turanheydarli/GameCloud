package rtapi

import (
	"context"

	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
	"google.golang.org/protobuf/proto"
)

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
	case *pbrt.Envelope_UpdatePlayer:
		return "update_player"
	case *pbrt.Envelope_UpdatePlayerAttributes:
		return "update_player_attributes"
	case *pbrt.Envelope_GetPlayerAttributes:
		return "get_player_attributes"
	case *pbrt.Envelope_DeletePlayerAttribute:
		return "delete_player_attribute"
	default:
		return "unknown"
	}
}

func (h *Handler) processEnvelope(session *ClientSession, envelope *pbrt.Envelope) {
	ctx := context.WithValue(session.Ctx, sessionContextKey, session)

	opCode := getOpCode(envelope)
	h.log.Infow("received message", "op_code", opCode, "session_id", session.ID)

	switch opCode {
	case "connect":
		h.handleAuthentication(ctx, session, envelope)
	case "matchmaker_add":
		h.handleMatchmakingCreateTicket(ctx, session, envelope)
	case "update_player":
		h.handleUpdatePlayer(ctx, session, envelope)
	case "update_player_attributes":
		h.handleUpdatePlayerAttributes(ctx, session, envelope)
	case "get_player_attributes":
		h.handleGetPlayerAttributes(ctx, session, envelope)
	case "delete_player_attribute":
		h.handleDeletePlayerAttribute(ctx, session, envelope)
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
