package rtapi

import (
	"context"

	"github.com/turanheydarli/gamecloud/relay/internal/session"
	"github.com/turanheydarli/gamecloud/relay/internal/transport"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
)

func (h *Handler) handleAuthentication(ctx context.Context, session *session.ClientSession, envelope *pbrt.Envelope) {
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

	grpcCtx := transport.CreateGRPCContext(ctx, session.GameKey)

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
