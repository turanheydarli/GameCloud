package rtapi

import (
	"context"
	"encoding/json"

	"github.com/turanheydarli/gamecloud/relay/internal/room"
	"github.com/turanheydarli/gamecloud/relay/internal/rpc"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
)

// handleRPC processes RPC calls from clients
func (h *Handler) handleRPC(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope) {
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

	// Handle server-side RPC functions
	if rpc.RPCTarget(rpcCall.Target) == rpc.RPCTargetServer {
		h.handleServerRPC(ctx, session, envelope, rpcCall)
		return
	}

	// For client-to-client RPCs, player must be in a room
	room, inRoom := h.roomService.GetPlayerRoom(session.PlayerID)
	if !inRoom {
		h.sendErrorToSession(session, envelope.Id, "not_in_room", "You must be in a room to make client RPC calls")
		return
	}

	// Handle client-to-client RPC
	h.handleClientRPC(ctx, session, envelope, rpcCall, room)
}

// handleServerRPC processes server-side RPC calls
func (h *Handler) handleServerRPC(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope, rpcCall *pbrt.RPC) {
	// Call the server function
	result, err := h.rpcService.CallServerFunction(ctx, session.PlayerID, rpcCall.Method, rpcCall.Params)
	if err != nil {
		h.sendErrorToSession(session, envelope.Id, "rpc_error", err.Error())
		return
	}

	// Send response back to the client
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

// handleClientRPC processes client-to-client RPC calls
func (h *Handler) handleClientRPC(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope, rpcCall *pbrt.RPC, room *room.Room) {
	// Create the RPC event to broadcast
	rpcEvent := h.rpcService.CreateRPCEvent(
		rpcCall.Id,
		session.PlayerID,
		rpcCall.Method,
		rpcCall.Params,
		rpcCall.ViewId,
	)

	// Send acknowledgment to the sender
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

	// Determine targets based on the RPC target type
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
		// Send to specific players
		var targets []string
		if err := json.Unmarshal(rpcCall.TargetPlayers, &targets); err == nil {
			for _, targetID := range targets {
				h.sendRPCToPlayer(targetID, rpcEvent)
			}
		}
	}
}

// broadcastRPCToRoom broadcasts an RPC event to all players in a room
func (h *Handler) broadcastRPCToRoom(roomID string, rpcEvent *pbrt.RPCEvent, includeSender bool) {
	players := h.roomService.GetPlayersInRoom(roomID)

	for _, playerID := range players {
		if !includeSender && playerID == rpcEvent.SenderId {
			continue // Skip sender if not including them
		}

		h.sendRPCToPlayer(playerID, rpcEvent)
	}
}

// sendRPCToPlayer sends an RPC event to a specific player
func (h *Handler) sendRPCToPlayer(playerID string, rpcEvent *pbrt.RPCEvent) {
	envelope := &pbrt.Envelope{
		Id: generateSessionID(),
		Message: &pbrt.Envelope_RpcEvent{
			RpcEvent: rpcEvent,
		},
	}

	h.SendToPlayer(playerID, envelope)
}
