package rpc

import (
	"context"
	"errors"
	"sync"

	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
)

type RPCTarget int

const (
	RPCTargetAll RPCTarget = iota
	RPCTargetOthers
	RPCTargetMaster
	RPCTargetSpecific
	RPCTargetServer
)

type ServerFunc func(ctx context.Context, playerID string, params []byte) ([]byte, error)

type Service struct {
	log         logger.Logger
	serverFuncs map[string]ServerFunc
	mutex       sync.RWMutex
}

func NewService(log logger.Logger) *Service {
	return &Service{
		log:         log,
		serverFuncs: make(map[string]ServerFunc),
	}
}

func (s *Service) RegisterServerFunction(name string, fn ServerFunc) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	s.serverFuncs[name] = fn
	s.log.Infow("registered server RPC function", "name", name)
}

func (s *Service) CallServerFunction(ctx context.Context, playerID, name string, params []byte) ([]byte, error) {
	s.mutex.RLock()
	fn, exists := s.serverFuncs[name]
	s.mutex.RUnlock()

	if !exists {
		s.log.Warnw("unknown server RPC function", "name", name, "player_id", playerID)
		return nil, ErrUnknownFunction
	}

	s.log.Infow("calling server RPC function", "name", name, "player_id", playerID)

	result, err := fn(ctx, playerID, params)
	if err != nil {
		s.log.Errorw("server RPC function failed",
			"name", name,
			"player_id", playerID,
			"error", err)
		return nil, err
	}

	return result, nil
}

func (s *Service) CreateRPCEvent(id, senderID, method string, params []byte, viewID int32) *pbrt.RPCEvent {
	return &pbrt.RPCEvent{
		Id:       id,
		SenderId: senderID,
		Method:   method,
		Params:   params,
		ViewId:   viewID,
	}
}

var (
	ErrUnknownFunction = errors.New("unknown RPC function")
)
