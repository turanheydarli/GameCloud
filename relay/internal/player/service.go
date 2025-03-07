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

func (s *Service) UpdatePlayer(ctx context.Context, req *pbrt.UpdatePlayerRequest) (*pbrt.UpdatePlayerResponse, error) {
	resp, err := s.grpcClient.UpdatePlayer(ctx, req)
	if err != nil {
		if st, ok := status.FromError(err); ok {
			s.log.Warnw("player update failed", "grpc_code", st.Code(), "message", st.Message())
			return nil, fmt.Errorf("player update failed: %s", st.Message())
		}
		s.log.Errorw("unexpected error during player update", "error", err)
		return nil, fmt.Errorf("unexpected error: %w", err)
	}

	return resp, nil
}

func (s *Service) UpdatePlayerAttributes(ctx context.Context, req *pbrt.UpdatePlayerAttributesRequest) (*pbrt.UpdatePlayerAttributesResponse, error) {
	resp, err := s.grpcClient.UpdatePlayerAttributes(ctx, req)
	if err != nil {
		if st, ok := status.FromError(err); ok {
			s.log.Warnw("player attributes update failed", "grpc_code", st.Code(), "message", st.Message())
			return nil, fmt.Errorf("player attributes update failed: %s", st.Message())
		}
		s.log.Errorw("unexpected error during player attributes update", "error", err)
		return nil, fmt.Errorf("unexpected error: %w", err)
	}

	return resp, nil
}

func (s *Service) GetPlayerAttributes(ctx context.Context, req *pbrt.GetPlayerAttributesRequest) (*pbrt.GetPlayerAttributesResponse, error) {
	resp, err := s.grpcClient.GetPlayerAttributes(ctx, req)
	if err != nil {
		if st, ok := status.FromError(err); ok {
			s.log.Warnw("get player attributes failed", "grpc_code", st.Code(), "message", st.Message())
			return nil, fmt.Errorf("get player attributes failed: %s", st.Message())
		}
		s.log.Errorw("unexpected error during get player attributes", "error", err)
		return nil, fmt.Errorf("unexpected error: %w", err)
	}

	return resp, nil
}

func (s *Service) DeletePlayerAttribute(ctx context.Context, req *pbrt.DeletePlayerAttributeRequest) (*pbrt.DeletePlayerAttributeResponse, error) {
	resp, err := s.grpcClient.DeletePlayerAttribute(ctx, req)
	if err != nil {
		if st, ok := status.FromError(err); ok {
			s.log.Warnw("delete player attribute failed", "grpc_code", st.Code(), "message", st.Message())
			return nil, fmt.Errorf("delete player attribute failed: %s", st.Message())
		}
		s.log.Errorw("unexpected error during delete player attribute", "error", err)
		return nil, fmt.Errorf("unexpected error: %w", err)
	}

	return resp, nil
}
