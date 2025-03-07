package rpc

import (
	"context"
	"errors"
	"sync"

	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
)

// RPCTarget defines who should receive the RPC call
type RPCTarget int

const (
	RPCTargetAll      RPCTarget = iota // Send to all clients including sender
	RPCTargetOthers                    // Send to all clients except sender
	RPCTargetMaster                    // Send to room master/owner only
	RPCTargetSpecific                  // Send to specific players
	RPCTargetServer                    // Call a server-side function
)

// ServerFunc is a server-side RPC function that can be called by clients
type ServerFunc func(ctx context.Context, playerID string, params []byte) ([]byte, error)

// Service manages RPC calls
type Service struct {
	log         logger.Logger
	serverFuncs map[string]ServerFunc
	mutex       sync.RWMutex
}

// NewService creates a new RPC service
func NewService(log logger.Logger) *Service {
	return &Service{
		log:         log,
		serverFuncs: make(map[string]ServerFunc),
	}
}

// RegisterServerFunction registers a server-side RPC function
func (s *Service) RegisterServerFunction(name string, fn ServerFunc) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	s.serverFuncs[name] = fn
	s.log.Infow("registered server RPC function", "name", name)
}

// CallServerFunction calls a server-side RPC function
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

// CreateRPCEvent creates an RPC event for broadcasting
func (s *Service) CreateRPCEvent(id, senderID, method string, params []byte, viewID int32) *pbrt.RPCEvent {
	return &pbrt.RPCEvent{
		Id:       id,
		SenderId: senderID,
		Method:   method,
		Params:   params,
		ViewId:   viewID,
	}
}

// Errors
var (
	ErrUnknownFunction = errors.New("unknown RPC function")
)
