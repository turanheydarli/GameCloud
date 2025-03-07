package rtapi

import (
	"context"

	"github.com/turanheydarli/gamecloud/relay/internal/sync"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
)

// handleObjectInstantiate processes object instantiation requests
func (h *Handler) handleObjectInstantiate(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to instantiate objects")
		return
	}

	instMsg, ok := envelope.Message.(*pbrt.Envelope_ObjectInstantiate)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid instantiate payload")
		return
	}

	// Get player's current room
	room, inRoom := h.roomService.GetPlayerRoom(session.PlayerID)
	if !inRoom {
		h.sendErrorToSession(session, envelope.Id, "not_in_room", "You must be in a room to instantiate objects")
		return
	}

	// Create the object using the sync service
	obj := h.syncService.CreateObject(
		session.PlayerID,
		room.ID,
		instMsg.ObjectInstantiate.PrefabName,
		instMsg.ObjectInstantiate.Position,
		instMsg.ObjectInstantiate.Rotation,
		instMsg.ObjectInstantiate.Scale,
		instMsg.ObjectInstantiate.Properties,
	)

	// Send response to instantiator
	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_ObjectInstantiate{
			ObjectInstantiate: &pbrt.ObjectInstantiate{
				ViewId:     obj.ViewID,
				OwnerId:    obj.OwnerID,
				PrefabName: obj.PrefabName,
				Position:   obj.Position,
				Rotation:   obj.Rotation,
				Scale:      obj.Scale,
				Properties: obj.Properties,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)

	// Broadcast to other players in the room
	h.broadcastObjectInstantiate(session.PlayerID, obj)
}

// handleObjectSync processes object synchronization requests
func (h *Handler) handleObjectSync(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to sync objects")
		return
	}

	syncMsg, ok := envelope.Message.(*pbrt.Envelope_ObjectSync)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid sync payload")
		return
	}

	// Get the object
	obj, exists := h.syncService.GetObject(syncMsg.ObjectSync.ViewId)
	if !exists {
		h.sendErrorToSession(session, envelope.Id, "object_not_found", "Object not found")
		return
	}

	// Check ownership (only owner can sync, unless it's a shared object)
	if obj.OwnerID != session.PlayerID && obj.OwnerID != "" {
		h.sendErrorToSession(session, envelope.Id, "not_owner", "You don't own this object")
		return
	}

	// Update transform if provided
	if syncMsg.ObjectSync.Position != nil || syncMsg.ObjectSync.Rotation != nil || syncMsg.ObjectSync.Scale != nil {
		h.syncService.UpdateObjectTransform(
			obj.ViewID,
			syncMsg.ObjectSync.Position,
			syncMsg.ObjectSync.Rotation,
			syncMsg.ObjectSync.Scale,
		)
	}

	// Update properties if provided
	if syncMsg.ObjectSync.Properties != nil && len(syncMsg.ObjectSync.Properties) > 0 {
		h.syncService.UpdateObjectProperties(obj.ViewID, syncMsg.ObjectSync.Properties)
	}

	// Send acknowledgment to sender
	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_ObjectSyncAck{
			ObjectSyncAck: &pbrt.ObjectSyncAck{
				ViewId:  obj.ViewID,
				Success: true,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)

	// Broadcast to other players in the room
	h.broadcastObjectSync(session.PlayerID, syncMsg.ObjectSync)
}

// handleObjectDestroy processes object destruction requests
func (h *Handler) handleObjectDestroy(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to destroy objects")
		return
	}

	destroyMsg, ok := envelope.Message.(*pbrt.Envelope_ObjectDestroy)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid destroy payload")
		return
	}

	// Get the object
	obj, exists := h.syncService.GetObject(destroyMsg.ObjectDestroy.ViewId)
	if !exists {
		h.sendErrorToSession(session, envelope.Id, "object_not_found", "Object not found")
		return
	}

	// Check ownership (only owner can destroy, unless it's a shared object)
	if obj.OwnerID != session.PlayerID && obj.OwnerID != "" {
		h.sendErrorToSession(session, envelope.Id, "not_owner", "You don't own this object")
		return
	}

	// Store room ID for broadcasting
	roomID := obj.RoomID
	viewID := obj.ViewID

	// Delete the object
	roomID, success := h.syncService.DeleteObject(viewID)
	if !success {
		h.sendErrorToSession(session, envelope.Id, "destroy_failed", "Failed to destroy object")
		return
	}

	// Send response to destroyer
	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_ObjectDestroy{
			ObjectDestroy: &pbrt.ObjectDestroy{
				ViewId:  viewID,
				Success: true,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)

	// Broadcast to other players in the room
	h.broadcastObjectDestroy(session.PlayerID, roomID, viewID)
}

// broadcastObjectInstantiate broadcasts object instantiation to all players in a room except the sender
func (h *Handler) broadcastObjectInstantiate(senderID string, obj *sync.NetworkObject) {
	players := h.roomService.GetPlayersInRoom(obj.RoomID)

	instantiateMsg := &pbrt.ObjectInstantiate{
		ViewId:     obj.ViewID,
		OwnerId:    obj.OwnerID,
		PrefabName: obj.PrefabName,
		Position:   obj.Position,
		Rotation:   obj.Rotation,
		Scale:      obj.Scale,
		Properties: obj.Properties,
	}

	for _, playerID := range players {
		if playerID == senderID {
			continue // Skip sender
		}

		envelope := &pbrt.Envelope{
			Id: generateSessionID(),
			Message: &pbrt.Envelope_ObjectInstantiate{
				ObjectInstantiate: instantiateMsg,
			},
		}

		h.SendToPlayer(playerID, envelope)
	}
}

// broadcastObjectSync broadcasts object sync to all players in a room except the sender
func (h *Handler) broadcastObjectSync(senderID string, syncMsg *pbrt.ObjectSync) {
	obj, exists := h.syncService.GetObject(syncMsg.ViewId)
	if !exists {
		return
	}

	players := h.roomService.GetPlayersInRoom(obj.RoomID)

	for _, playerID := range players {
		if playerID == senderID {
			continue // Skip sender
		}

		envelope := &pbrt.Envelope{
			Id: generateSessionID(),
			Message: &pbrt.Envelope_ObjectSync{
				ObjectSync: syncMsg,
			},
		}

		h.SendToPlayer(playerID, envelope)
	}
}

// broadcastObjectDestroy broadcasts object destruction to all players in a room except the sender
func (h *Handler) broadcastObjectDestroy(senderID string, roomID string, viewID int32) {
	players := h.roomService.GetPlayersInRoom(roomID)

	destroyMsg := &pbrt.ObjectDestroy{
		ViewId:  viewID,
		Success: true,
	}

	for _, playerID := range players {
		if playerID == senderID {
			continue // Skip sender
		}

		envelope := &pbrt.Envelope{
			Id: generateSessionID(),
			Message: &pbrt.Envelope_ObjectDestroy{
				ObjectDestroy: destroyMsg,
			},
		}

		h.SendToPlayer(playerID, envelope)
	}
}

// sendRoomObjectsToPlayer sends all existing objects in a room to a player
func (h *Handler) sendRoomObjectsToPlayer(playerID string, roomID string) {
	roomObjects := h.syncService.GetRoomObjects(roomID)

	for _, obj := range roomObjects {
		instantiateMsg := &pbrt.ObjectInstantiate{
			ViewId:     obj.ViewID,
			OwnerId:    obj.OwnerID,
			PrefabName: obj.PrefabName,
			Position:   obj.Position,
			Rotation:   obj.Rotation,
			Scale:      obj.Scale,
			Properties: obj.Properties,
		}

		envelope := &pbrt.Envelope{
			Id: generateSessionID(),
			Message: &pbrt.Envelope_ObjectInstantiate{
				ObjectInstantiate: instantiateMsg,
			},
		}

		h.SendToPlayer(playerID, envelope)
	}
}
