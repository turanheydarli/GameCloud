package rtapi

import (
	"context"

	"github.com/turanheydarli/gamecloud/relay/internal/session"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
)

func (h *Handler) handleMatchmakingCreateTicket(ctx context.Context, session *session.ClientSession, envelope *pbrt.Envelope) {
	// Empty implementation as in original code
}
