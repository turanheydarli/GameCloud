package matchmaking

import (
	"context"
	"fmt"
	"time"

	"github.com/turanheydarli/gamecloud/relay/internal/rtapi"
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	pbrt "github.com/turanheydarli/gamecloud/proto" 
)

type Service struct {
	log       logger.Logger
	wsManager *rtapi.WebSocketManager
}

func NewService(log logger.Logger, wsManager *rtapi.WebSocketManager) *Service {
	return &Service{
		log:       log,
		wsManager: wsManager,
	}
}

func (s *Service) CreateTicket(ctx context.Context, playerID string, queueName string) (string, error) {
	// Here you'd call your ASP.NET (REST or gRPC) to create a ticket.
	ticketID := fmt.Sprintf("ticket-%d", time.Now().UnixNano())
	s.log.Infow("CreateTicket", "player", playerID, "queue", queueName, "ticket", ticketID)
	return ticketID, nil
}

func (s *Service) JoinMatch(ctx context.Context, playerID, matchID string) error {
	s.log.Infow("JoinMatch", "player", playerID, "match", matchID)
	return nil
}

func (s *Service) ProcessMatchmaking(ctx context.Context, queueName string) error {
	s.log.Infow("ProcessMatchmaking", "queue", queueName)
	return nil
}

func (s *Service) NotifyBattleStart(matchID string, playerIDs []string) {
	for _, pid := range playerIDs {
		message := &pbrt.Envelope{
			Id: "battle-start",
			Message: &pbrt.Envelope_Status{
				Status: &pbrt.Status{
					PresenceStatuses: map[string]string{
						pid: "BattleStarted",
					},
				},
			},
		}
		s.wsManager.SendToPlayer(pid, message)
	}
} 