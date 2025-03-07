package rtapi

import (
	"context"
	"encoding/json"

	"github.com/turanheydarli/gamecloud/relay/internal/room"
	"github.com/turanheydarli/gamecloud/relay/internal/rpc"
	"github.com/turanheydarli/gamecloud/relay/internal/session"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
)

func (h *Handler) handleRPC(ctx context.Context, session *session.ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to make RPC calls")
		return
	}

	rpcMsg, ok := envelope.Message.(*pbrt.Envelope_Rpc)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid RPC payload")
		return
	}

	rpcCall := rpcMsg.Rpc
	h.log.Infow("received RPC call",
		"method", rpcCall.Method,
		"target", rpcCall.Target,
		"session_id", session.ID,
		"player_id", session.PlayerID)

	if rpc.RPCTarget(rpcCall.Target) == rpc.RPCTargetServer {
		h.handleServerRPC(ctx, session, envelope, rpcCall)
		return
	}

	room, inRoom := h.roomService.GetPlayerRoom(session.PlayerID)
	if !inRoom {
		h.sendErrorToSession(session, envelope.Id, "not_in_room", "You must be in a room to make client RPC calls")
		return
	}

	h.handleClientRPC(ctx, session, envelope, rpcCall, room)
}

func (h *Handler) handleServerRPC(ctx context.Context, session *session.ClientSession, envelope *pbrt.Envelope, rpcCall *pbrt.RPC) {
	result, err := h.rpcService.CallServerFunction(ctx, session.PlayerID, rpcCall.Method, rpcCall.Params)
	if err != nil {
		h.sendErrorToSession(session, envelope.Id, "rpc_error", err.Error())
		return
	}

	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_RpcResult{
			RpcResult: &pbrt.RPCResult{
				Id:     rpcCall.Id,
				Result: result,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)
}

func (h *Handler) handleClientRPC(ctx context.Context, session *session.ClientSession, envelope *pbrt.Envelope, rpcCall *pbrt.RPC, room *room.Room) {
	rpcEvent := h.rpcService.CreateRPCEvent(
		rpcCall.Id,
		session.PlayerID,
		rpcCall.Method,
		rpcCall.Params,
		rpcCall.ViewId,
	)

	ackResponse := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_RpcResult{
			RpcResult: &pbrt.RPCResult{
				Id:     rpcCall.Id,
				Result: []byte(`{"success":true}`),
			},
		},
	}

	h.sendEnvelopeToSession(session, ackResponse)

	switch rpc.RPCTarget(rpcCall.Target) {
	case rpc.RPCTargetAll:
		// Send to all players in the room including sender
		h.broadcastRPCToRoom(room.ID, rpcEvent, true)

	case rpc.RPCTargetOthers:
		// Send to all players in the room except sender
		h.broadcastRPCToRoom(room.ID, rpcEvent, false)

	case rpc.RPCTargetMaster:
		// Send only to the room owner
		if room.OwnerID != "" {
			h.sendRPCToPlayer(room.OwnerID, rpcEvent)
		}

	case rpc.RPCTargetSpecific:
		var targets []string
		if err := json.Unmarshal(rpcCall.TargetPlayers, &targets); err == nil {
			for _, targetID := range targets {
				h.sendRPCToPlayer(targetID, rpcEvent)
			}
		}
	}
}

func (h *Handler) broadcastRPCToRoom(roomID string, rpcEvent *pbrt.RPCEvent, includeSender bool) {
	players := h.roomService.GetPlayersInRoom(roomID)

	for _, playerID := range players {
		if !includeSender && playerID == rpcEvent.SenderId {
			continue // Skip sender if not including them
		}

		h.sendRPCToPlayer(playerID, rpcEvent)
	}
}

func (h *Handler) sendRPCToPlayer(playerID string, rpcEvent *pbrt.RPCEvent) {
	envelope := &pbrt.Envelope{
		Id: generateSessionID(),
		Message: &pbrt.Envelope_RpcEvent{
			RpcEvent: rpcEvent,
		},
	}

	h.SendToPlayer(playerID, envelope)
}
