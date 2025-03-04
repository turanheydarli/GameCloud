package player

import (
	"context"
	"fmt"

	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
	"google.golang.org/grpc"
	"google.golang.org/grpc/status"
)

type Service struct {
	log        logger.Logger
	grpcClient pbrt.RelayServiceClient
}

func NewService(log logger.Logger, conn *grpc.ClientConn) *Service {
	return &Service{
		log:        log,
		grpcClient: pbrt.NewRelayServiceClient(conn),
	}
}

func (s *Service) Authenticate(ctx context.Context, req *pbrt.AuthenticateRequest) (*pbrt.AuthenticateResponse, error) {
	s.log.Infow("authenticating player", "device_id_present", req.DeviceId != "", "custom_id_present", req.CustomId != "")

	resp, err := s.grpcClient.AuthenticateUser(ctx, req)
	if err != nil {
		if st, ok := status.FromError(err); ok {
			s.log.Warnw("authentication failed", "grpc_code", st.Code(), "message", st.Message())
			return nil, fmt.Errorf("authentication failed: %s", st.Message())
		}
		s.log.Errorw("unexpected error during authentication", "error", err)
		return nil, fmt.Errorf("unexpected error: %w", err)
	}

	return resp, nil
}

func (s *Service) ValidateToken(ctx context.Context, req *pbrt.ValidateTokenRequest) (*pbrt.ValidateTokenResponse, error) {
	s.log.Infow("validating token", "token_present", req.Token != "")

	resp, err := s.grpcClient.ValidateToken(ctx, req)
	if err != nil {
		if st, ok := status.FromError(err); ok {
			s.log.Warnw("token validation failed", "grpc_code", st.Code(), "message", st.Message())
			return nil, fmt.Errorf("token validation failed: %s", st.Message())
		}
		s.log.Errorw("unexpected error during token validation", "error", err)
		return nil, fmt.Errorf("unexpected error: %w", err)
	}

	return resp, nil
}
