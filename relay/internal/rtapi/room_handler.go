package rtapi

import (
	"context"

	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
)

func (h *Handler) handleRoomCreate(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to create rooms")
		return
	}

	createMsg, ok := envelope.Message.(*pbrt.Envelope_RoomCreate)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid room create payload")
		return
	}

	room := h.roomService.CreateRoom(
		createMsg.RoomCreate.Name,
		createMsg.RoomCreate.GameType,
		int(createMsg.RoomCreate.MaxPlayers),
		createMsg.RoomCreate.IsPrivate,
		createMsg.RoomCreate.Password,
		session.PlayerID,
		createMsg.RoomCreate.Metadata,
	)

	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_RoomCreate{
			RoomCreate: &pbrt.RoomCreate{
				RoomId:     room.ID,
				Name:       room.Name,
				GameType:   room.GameType,
				MaxPlayers: int32(room.MaxPlayers),
				IsPrivate:  room.IsPrivate,
				Metadata:   room.Metadata,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)
}

func (h *Handler) handleRoomJoin(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to join rooms")
		return
	}

	joinMsg, ok := envelope.Message.(*pbrt.Envelope_RoomJoin)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid room join payload")
		return
	}

	room, exists := h.roomService.GetRoom(joinMsg.RoomJoin.RoomId)
	if !exists {
		h.sendErrorToSession(session, envelope.Id, "room_not_found", "Room not found")
		return
	}

	if room.IsPrivate && room.Password != joinMsg.RoomJoin.Password {
		h.sendErrorToSession(session, envelope.Id, "invalid_password", "Invalid room password")
		return
	}

	room, success := h.roomService.JoinRoom(
		room.ID,
		session.PlayerID,
		session.ID,
		joinMsg.RoomJoin.Metadata,
	)

	if !success {
		h.sendErrorToSession(session, envelope.Id, "join_failed", "Failed to join room")
		return
	}

	players := make([]*pbrt.RoomPlayer, 0, len(room.Players))
	for id, player := range room.Players {
		players = append(players, &pbrt.RoomPlayer{
			PlayerId: id,
			IsReady:  player.IsReady,
			Metadata: player.Metadata,
		})
	}

	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_RoomJoin{
			RoomJoin: &pbrt.RoomJoin{
				RoomId:   room.ID,
				Success:  true,
				Players:  players,
				Metadata: room.Metadata,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)

	h.broadcastPlayerJoined(session.PlayerID, room.ID)

	h.sendRoomObjectsToPlayer(session.PlayerID, room.ID)
}

func (h *Handler) handleRoomLeave(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to leave rooms")
		return
	}

	leaveMsg, ok := envelope.Message.(*pbrt.Envelope_RoomLeave)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid room leave payload")
		return
	}

	currentRoom, inRoom := h.roomService.GetPlayerRoom(session.PlayerID)
	if !inRoom {
		h.sendErrorToSession(session, envelope.Id, "not_in_room", "You are not in a room")
		return
	}

	if leaveMsg.RoomLeave.RoomId != "" && leaveMsg.RoomLeave.RoomId != currentRoom.ID {
		h.sendErrorToSession(session, envelope.Id, "wrong_room", "You are not in the specified room")
		return
	}

	roomID := currentRoom.ID

	_, success := h.roomService.LeaveRoom(session.PlayerID)
	if !success {
		h.sendErrorToSession(session, envelope.Id, "leave_failed", "Failed to leave room")
		return
	}

	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_RoomLeave{
			RoomLeave: &pbrt.RoomLeave{
				RoomId:  roomID,
				Success: true,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)

	h.broadcastPlayerLeft(session.PlayerID, roomID)
}

func (h *Handler) handleRoomMessage(ctx context.Context, session *ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to send room messages")
		return
	}

	msgData, ok := envelope.Message.(*pbrt.Envelope_RoomMessage)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid room message payload")
		return
	}

	currentRoom, inRoom := h.roomService.GetPlayerRoom(session.PlayerID)
	if !inRoom {
		h.sendErrorToSession(session, envelope.Id, "not_in_room", "You are not in a room")
		return
	}

	if msgData.RoomMessage.RoomId != currentRoom.ID {
		h.sendErrorToSession(session, envelope.Id, "wrong_room", "You are not in the specified room")
		return
	}

	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_RoomMessage{
			RoomMessage: &pbrt.RoomMessage{
				RoomId:  currentRoom.ID,
				Success: true,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)

	h.broadcastRoomMessage(session.PlayerID, currentRoom.ID, msgData.RoomMessage.Data)
}

func (h *Handler) broadcastPlayerJoined(playerID string, roomID string) {
	players := h.roomService.GetPlayersInRoom(roomID)

	for _, pid := range players {
		if pid == playerID {
			continue
		}

		playerJoinedMsg := &pbrt.PlayerJoined{
			RoomId:   roomID,
			PlayerId: playerID,
		}

		envelope := &pbrt.Envelope{
			Id: generateSessionID(),
			Message: &pbrt.Envelope_PlayerJoined{
				PlayerJoined: playerJoinedMsg,
			},
		}

		h.SendToPlayer(pid, envelope)
	}
}

func (h *Handler) broadcastPlayerLeft(playerID string, roomID string) {
	players := h.roomService.GetPlayersInRoom(roomID)

	playerLeftMsg := &pbrt.PlayerLeft{
		RoomId:   roomID,
		PlayerId: playerID,
	}

	for _, pid := range players {
		envelope := &pbrt.Envelope{
			Id: generateSessionID(),
			Message: &pbrt.Envelope_PlayerLeft{
				PlayerLeft: playerLeftMsg,
			},
		}

		h.SendToPlayer(pid, envelope)
	}
}

func (h *Handler) broadcastRoomMessage(senderID string, roomID string, data []byte) {
	players := h.roomService.GetPlayersInRoom(roomID)

	roomMsgEvent := &pbrt.RoomMessageEvent{
		RoomId:   roomID,
		SenderId: senderID,
		Data:     data,
	}

	for _, pid := range players {
		if pid == senderID {
			continue
		}

		envelope := &pbrt.Envelope{
			Id: generateSessionID(),
			Message: &pbrt.Envelope_RoomMessageEvent{
				RoomMessageEvent: roomMsgEvent,
			},
		}

		h.SendToPlayer(pid, envelope)
	}
}
