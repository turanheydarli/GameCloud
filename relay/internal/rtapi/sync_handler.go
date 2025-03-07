package rtapi

import (
	"context"

	"github.com/turanheydarli/gamecloud/relay/internal/session"
	"github.com/turanheydarli/gamecloud/relay/internal/sync"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
)

func (h *Handler) handleObjectInstantiate(ctx context.Context, session *session.ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to instantiate objects")
		return
	}

	instMsg, ok := envelope.Message.(*pbrt.Envelope_ObjectInstantiate)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid instantiate payload")
		return
	}

	room, inRoom := h.roomService.GetPlayerRoom(session.PlayerID)
	if !inRoom {
		h.sendErrorToSession(session, envelope.Id, "not_in_room", "You must be in a room to instantiate objects")
		return
	}

	obj := h.syncService.CreateObject(
		session.PlayerID,
		room.ID,
		instMsg.ObjectInstantiate.PrefabName,
		instMsg.ObjectInstantiate.Position,
		instMsg.ObjectInstantiate.Rotation,
		instMsg.ObjectInstantiate.Scale,
		instMsg.ObjectInstantiate.Properties,
	)

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

	h.broadcastObjectInstantiate(session.PlayerID, obj)
}

func (h *Handler) handleObjectSync(ctx context.Context, session *session.ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to sync objects")
		return
	}

	syncMsg, ok := envelope.Message.(*pbrt.Envelope_ObjectSync)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid sync payload")
		return
	}

	obj, exists := h.syncService.GetObject(syncMsg.ObjectSync.ViewId)
	if !exists {
		h.sendErrorToSession(session, envelope.Id, "object_not_found", "Object not found")
		return
	}

	if obj.OwnerID != session.PlayerID && obj.OwnerID != "" {
		h.sendErrorToSession(session, envelope.Id, "not_owner", "You don't own this object")
		return
	}

	if syncMsg.ObjectSync.Position != nil || syncMsg.ObjectSync.Rotation != nil || syncMsg.ObjectSync.Scale != nil {
		h.syncService.UpdateObjectTransform(
			obj.ViewID,
			syncMsg.ObjectSync.Position,
			syncMsg.ObjectSync.Rotation,
			syncMsg.ObjectSync.Scale,
		)
	}

	if syncMsg.ObjectSync.Properties != nil && len(syncMsg.ObjectSync.Properties) > 0 {
		h.syncService.UpdateObjectProperties(obj.ViewID, syncMsg.ObjectSync.Properties)
	}

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

	h.broadcastObjectSync(session.PlayerID, syncMsg.ObjectSync)
}

func (h *Handler) handleObjectDestroy(ctx context.Context, session *session.ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to destroy objects")
		return
	}

	destroyMsg, ok := envelope.Message.(*pbrt.Envelope_ObjectDestroy)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid destroy payload")
		return
	}

	obj, exists := h.syncService.GetObject(destroyMsg.ObjectDestroy.ViewId)
	if !exists {
		h.sendErrorToSession(session, envelope.Id, "object_not_found", "Object not found")
		return
	}

	if obj.OwnerID != session.PlayerID && obj.OwnerID != "" {
		h.sendErrorToSession(session, envelope.Id, "not_owner", "You don't own this object")
		return
	}

	roomID := obj.RoomID
	viewID := obj.ViewID

	roomID, success := h.syncService.DeleteObject(viewID)
	if !success {
		h.sendErrorToSession(session, envelope.Id, "destroy_failed", "Failed to destroy object")
		return
	}

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

	h.broadcastObjectDestroy(session.PlayerID, roomID, viewID)
}

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
			continue
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

func (h *Handler) broadcastObjectSync(senderID string, syncMsg *pbrt.ObjectSync) {
	obj, exists := h.syncService.GetObject(syncMsg.ViewId)
	if !exists {
		return
	}

	players := h.roomService.GetPlayersInRoom(obj.RoomID)

	for _, playerID := range players {
		if playerID == senderID {
			continue
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
