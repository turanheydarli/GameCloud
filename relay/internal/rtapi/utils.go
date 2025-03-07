package rtapi

import (
	"errors"

	"github.com/google/uuid"
	"github.com/turanheydarli/gamecloud/relay/internal/session"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
	"google.golang.org/protobuf/proto"
)

var ErrPlayerNotConnected = errors.New("player not connected")

// generateSessionID generates a unique session ID
func generateSessionID() string {
	return uuid.New().String()
}

func (h *Handler) handleGRPCError(session *session.ClientSession, envelopeID string, err error) {
	st, ok := status.FromError(err)
	if !ok {
		h.log.Errorw("non-gRPC error occurred", "error", err, "session_id", session.ID)
		h.sendErrorToSession(session, envelopeID, "internal_error", "An internal error occurred")
		return
	}

	var code string
	var message string

	switch st.Code() {
	case codes.Unauthenticated:
		code = "authentication_failed"
		message = st.Message()
	case codes.PermissionDenied:
		code = "permission_denied"
		message = st.Message()
	case codes.InvalidArgument:
		code = "invalid_argument"
		message = st.Message()
	case codes.NotFound:
		code = "not_found"
		message = st.Message()
	case codes.AlreadyExists:
		code = "already_exists"
		message = st.Message()
	case codes.ResourceExhausted:
		code = "resource_exhausted"
		message = st.Message()
	case codes.FailedPrecondition:
		code = "failed_precondition"
		message = st.Message()
	case codes.Aborted:
		code = "aborted"
		message = st.Message()
	case codes.DeadlineExceeded:
		code = "timeout"
		message = "The operation timed out"
	case codes.Unavailable:
		code = "service_unavailable"
		message = "The service is currently unavailable"
	default:
		code = "internal_error"
		message = "An internal error occurred"
	}

	h.log.Errorw("gRPC error occurred",
		"error", err,
		"grpc_code", st.Code(),
		"grpc_message", st.Message(),
		"client_code", code,
		"session_id", session.ID)

	var details []string
	for _, detail := range st.Details() {
		if d, ok := detail.(proto.Message); ok {
			detailBytes, err := proto.Marshal(d)
			if err == nil {
				details = append(details, string(detailBytes))
			}
		}
	}

	h.sendErrorToSession(session, envelopeID, code, message)
}
