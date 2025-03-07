package rtapi

import (
	"context"

	"github.com/turanheydarli/gamecloud/relay/internal/session"
	"github.com/turanheydarli/gamecloud/relay/internal/transport"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
)

func (h *Handler) handleUpdatePlayer(ctx context.Context, session *session.ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to update player data")
		return
	}

	updateMsg, ok := envelope.Message.(*pbrt.Envelope_UpdatePlayer)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid update player payload")
		return
	}

	grpcCtx := transport.CreateGRPCContext(ctx, session.GameKey)

	updateReq := &pbrt.UpdatePlayerRequest{
		PlayerId:    session.PlayerID,
		DisplayName: updateMsg.UpdatePlayer.DisplayName,
		Metadata:    updateMsg.UpdatePlayer.Metadata,
	}

	updateResp, err := h.playerService.UpdatePlayer(grpcCtx, updateReq)
	if err != nil {
		h.handleGRPCError(session, envelope.Id, err)
		return
	}

	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_UpdatePlayer{
			UpdatePlayer: &pbrt.UpdatePlayer{
				PlayerId:    updateResp.PlayerId,
				DisplayName: updateResp.DisplayName,
				AvatarUrl:   updateResp.AvatarUrl,
				Metadata:    updateResp.Metadata,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)

	h.log.Infow("player updated",
		"session_id", session.ID,
		"player_id", session.PlayerID)
}

func (h *Handler) handleUpdatePlayerAttributes(ctx context.Context, session *session.ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to update player attributes")
		return
	}

	updateMsg, ok := envelope.Message.(*pbrt.Envelope_UpdatePlayerAttributes)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid update player attributes payload")
		return
	}

	grpcCtx := transport.CreateGRPCContext(ctx, session.GameKey)

	updateReq := &pbrt.UpdatePlayerAttributesRequest{
		PlayerId:   session.PlayerID,
		Collection: updateMsg.UpdatePlayerAttributes.Collection,
		Key:        updateMsg.UpdatePlayerAttributes.Key,
		Value:      updateMsg.UpdatePlayerAttributes.Value,
	}

	updateResp, err := h.playerService.UpdatePlayerAttributes(grpcCtx, updateReq)
	if err != nil {
		h.handleGRPCError(session, envelope.Id, err)
		return
	}

	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_UpdatePlayerAttributes{
			UpdatePlayerAttributes: &pbrt.UpdatePlayerAttributes{
				PlayerId:   updateResp.PlayerId,
				Collection: updateResp.Collection,
				Key:        updateResp.Key,
				Value:      updateResp.Value,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)

	h.log.Infow("player attributes updated",
		"session_id", session.ID,
		"player_id", session.PlayerID,
		"collection", updateMsg.UpdatePlayerAttributes.Collection,
		"key", updateMsg.UpdatePlayerAttributes.Key)
}

func (h *Handler) handleGetPlayerAttributes(ctx context.Context, session *session.ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to get player attributes")
		return
	}

	getMsg, ok := envelope.Message.(*pbrt.Envelope_GetPlayerAttributes)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid get player attributes payload")
		return
	}

	grpcCtx := transport.CreateGRPCContext(ctx, session.GameKey)

	getReq := &pbrt.GetPlayerAttributesRequest{
		PlayerId:   session.PlayerID,
		Collection: getMsg.GetPlayerAttributes.Collection,
		Key:        getMsg.GetPlayerAttributes.Key,
	}

	getResp, err := h.playerService.GetPlayerAttributes(grpcCtx, getReq)
	if err != nil {
		h.handleGRPCError(session, envelope.Id, err)
		return
	}

	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_GetPlayerAttributes{
			GetPlayerAttributes: &pbrt.GetPlayerAttributes{
				PlayerId:   getResp.PlayerId,
				Collection: getResp.Collection,
				Key:        getResp.Key,
				Value:      getResp.Value,
				Attributes: getResp.Attributes,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)

	h.log.Infow("player attributes retrieved",
		"session_id", session.ID,
		"player_id", session.PlayerID,
		"collection", getMsg.GetPlayerAttributes.Collection)
}

func (h *Handler) handleDeletePlayerAttribute(ctx context.Context, session *session.ClientSession, envelope *pbrt.Envelope) {
	if session.PlayerID == "" {
		h.sendErrorToSession(session, envelope.Id, "unauthorized", "You must be authenticated to delete player attributes")
		return
	}

	deleteMsg, ok := envelope.Message.(*pbrt.Envelope_DeletePlayerAttribute)
	if !ok {
		h.sendErrorToSession(session, envelope.Id, "invalid_payload", "Invalid delete player attribute payload")
		return
	}

	grpcCtx := transport.CreateGRPCContext(ctx, session.GameKey)

	deleteReq := &pbrt.DeletePlayerAttributeRequest{
		PlayerId:   session.PlayerID,
		Collection: deleteMsg.DeletePlayerAttribute.Collection,
		Key:        deleteMsg.DeletePlayerAttribute.Key,
	}

	deleteResp, err := h.playerService.DeletePlayerAttribute(grpcCtx, deleteReq)
	if err != nil {
		h.handleGRPCError(session, envelope.Id, err)
		return
	}

	response := &pbrt.Envelope{
		Id: envelope.Id,
		Message: &pbrt.Envelope_DeletePlayerAttribute{
			DeletePlayerAttribute: &pbrt.DeletePlayerAttribute{
				PlayerId:   deleteResp.PlayerId,
				Collection: deleteResp.Collection,
				Key:        deleteResp.Key,
				Success:    deleteResp.Success,
			},
		},
	}

	h.sendEnvelopeToSession(session, response)

	h.log.Infow("player attribute deleted",
		"session_id", session.ID,
		"player_id", session.PlayerID,
		"collection", deleteMsg.DeletePlayerAttribute.Collection,
		"key", deleteMsg.DeletePlayerAttribute.Key)
}
