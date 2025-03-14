package room

import (
	"context"
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	"github.com/turanheydarli/gamecloud/relay/proto"
	"google.golang.org/grpc"
)

type Room struct {
	ID         string
	Name       string
	GameType   string
	MaxPlayers int
	IsPrivate  bool
	Password   string
	OwnerID    string
	Players    map[string]*RoomPlayer
	Metadata   map[string]string
	CreatedAt  time.Time
	UpdatedAt  time.Time
}

type RoomPlayer struct {
	PlayerID  string
	SessionID string
	IsReady   bool
	JoinedAt  time.Time
	Metadata  map[string]string
}

type Service struct {
	log         logger.Logger
	rooms       map[string]*Room
	playerRooms map[string]string
	mutex       sync.RWMutex
	grpcConn    *grpc.ClientConn
	grpcClient  proto.RelayServiceClient
}

func NewService(log logger.Logger) *Service {
	return &Service{
		log:         log,
		rooms:       make(map[string]*Room),
		playerRooms: make(map[string]string),
	}
}

func (s *Service) Initialize(conn *grpc.ClientConn) {
	s.grpcConn = conn
	s.grpcClient = proto.NewRelayServiceClient(conn)
	s.log.Infow("room service initialized with gRPC client")
}

func (s *Service) CreateRoom(name, gameType string, maxPlayers int, isPrivate bool,
	password string, ownerID string, metadata map[string]string) *Room {

	s.mutex.Lock()
	defer s.mutex.Unlock()

	config := &proto.RoomConfig{
		MinPlayers:       1,
		MaxPlayers:       int32(maxPlayers),
		TurnTimerSeconds: 60,
		AllowSpectators:  true,
		PersistState:     true,
		CustomConfig:     make(map[string]string),
	}

	req := &proto.CreateRoomRequest{
		GameId:     gameType,
		MaxPlayers: int32(maxPlayers),
		CreatorId:  ownerID,
		Private:    isPrivate,
		Label:      name,
		Metadata:   metadata,
		Config:     config,
	}

	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	protoRoom, err := s.grpcClient.CreateRoom(ctx, req)
	if err != nil {
		s.log.Errorw("failed to create room via gRPC", "error", err)
		return s.createLocalRoom(name, gameType, maxPlayers, isPrivate, password, ownerID, metadata)
	}

	// Convert proto room to local room
	room := &Room{
		ID:         protoRoom.Id,
		Name:       name,
		GameType:   gameType,
		MaxPlayers: maxPlayers,
		IsPrivate:  isPrivate,
		Password:   password,
		OwnerID:    ownerID,
		Players:    make(map[string]*RoomPlayer),
		Metadata:   protoRoom.Metadata,
		CreatedAt:  protoRoom.CreatedAt.AsTime(),
		UpdatedAt:  time.Now(),
	}

	// Add players from proto room
	for _, playerID := range protoRoom.PlayerIds {
		room.Players[playerID] = &RoomPlayer{
			PlayerID: playerID,
			IsReady:  false,
			JoinedAt: time.Now(),
			Metadata: make(map[string]string),
		}
		s.playerRooms[playerID] = room.ID
	}

	s.rooms[room.ID] = room
	s.log.Infow("room created via gRPC",
		"room_id", room.ID,
		"owner_id", ownerID,
		"game_type", gameType)

	return room
}

func (s *Service) createLocalRoom(name, gameType string, maxPlayers int, isPrivate bool,
	password string, ownerID string, metadata map[string]string) *Room {

	roomID := uuid.New().String()
	now := time.Now()

	room := &Room{
		ID:         roomID,
		Name:       name,
		GameType:   gameType,
		MaxPlayers: maxPlayers,
		IsPrivate:  isPrivate,
		Password:   password,
		OwnerID:    ownerID,
		Players:    make(map[string]*RoomPlayer),
		Metadata:   metadata,
		CreatedAt:  now,
		UpdatedAt:  now,
	}

	if ownerID != "" {
		room.Players[ownerID] = &RoomPlayer{
			PlayerID: ownerID,
			IsReady:  false,
			JoinedAt: now,
			Metadata: make(map[string]string),
		}
		s.playerRooms[ownerID] = roomID
	}

	s.rooms[roomID] = room

	s.log.Infow("room created locally (fallback)",
		"room_id", roomID,
		"owner_id", ownerID,
		"game_type", gameType)

	return room
}

func (s *Service) GetRoom(roomID string) (*Room, bool) {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	room, exists := s.rooms[roomID]
	if exists {
		return room, true
	}

	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	protoRoom, err := s.grpcClient.GetRoom(ctx, &proto.GetRoomRequest{
		RoomId: roomID,
	})

	if err != nil {
		s.log.Errorw("failed to get room via gRPC", "room_id", roomID, "error", err)
		return nil, false
	}

	room = &Room{
		ID:         protoRoom.Id,
		Name:       protoRoom.Id,
		GameType:   protoRoom.GameId,
		MaxPlayers: int(protoRoom.MaxPlayers),
		IsPrivate:  false,
		OwnerID:    protoRoom.CreatorId,
		Players:    make(map[string]*RoomPlayer),
		Metadata:   protoRoom.Metadata,
		CreatedAt:  protoRoom.CreatedAt.AsTime(),
		UpdatedAt:  time.Now(),
	}

	for _, playerID := range protoRoom.PlayerIds {
		room.Players[playerID] = &RoomPlayer{
			PlayerID: playerID,
			IsReady:  false,
			JoinedAt: time.Now(),
			Metadata: make(map[string]string),
		}
		s.playerRooms[playerID] = room.ID
	}

	s.rooms[room.ID] = room
	return room, true
}

func (s *Service) GetPlayerRoom(playerID string) (*Room, bool) {
	s.mutex.RLock()
	roomID, exists := s.playerRooms[playerID]
	if !exists {
		s.mutex.RUnlock()
		return nil, false
	}

	room, roomExists := s.rooms[roomID]
	s.mutex.RUnlock()

	return room, roomExists
}

func (s *Service) JoinRoom(roomID, playerID, sessionID string, metadata map[string]string) (*Room, bool) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	// Call gRPC service to join room
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	joinResp, err := s.grpcClient.JoinRoom(ctx, &proto.JoinRoomRequest{
		RoomId:      roomID,
		UserId:      playerID,
		SessionId:   sessionID,
		AsSpectator: false,
	})

	if err != nil || !joinResp.Success {
		s.log.Errorw("failed to join room via gRPC",
			"room_id", roomID,
			"player_id", playerID,
			"error", err,
			"success", joinResp != nil && joinResp.Success,
			"error_msg", func() string {
				if joinResp != nil {
					return joinResp.Error
				}
				return ""
			}())

		return s.joinRoomLocally(roomID, playerID, sessionID, metadata)
	}

	protoRoom := joinResp.Room

	room, exists := s.rooms[roomID]
	if !exists {
		room = &Room{
			ID:         protoRoom.Id,
			Name:       protoRoom.Id,
			GameType:   protoRoom.GameId,
			MaxPlayers: int(protoRoom.MaxPlayers),
			IsPrivate:  false,
			OwnerID:    protoRoom.CreatorId,
			Players:    make(map[string]*RoomPlayer),
			Metadata:   protoRoom.Metadata,
			CreatedAt:  protoRoom.CreatedAt.AsTime(),
			UpdatedAt:  time.Now(),
		}
		s.rooms[roomID] = room
	}

	room.Players = make(map[string]*RoomPlayer)
	for _, pid := range protoRoom.PlayerIds {
		isCurrentPlayer := pid == playerID
		room.Players[pid] = &RoomPlayer{
			PlayerID: pid,
			SessionID: func() string {
				if isCurrentPlayer {
					return sessionID
				}
				return ""
			}(),
			IsReady:  false,
			JoinedAt: time.Now(),
			Metadata: func() map[string]string {
				if isCurrentPlayer {
					return metadata
				}
				return make(map[string]string)
			}(),
		}
		s.playerRooms[pid] = roomID
	}

	s.log.Infow("player joined room via gRPC",
		"room_id", roomID,
		"player_id", playerID)

	return room, true
}

// Fallback method for local room join if gRPC fails
func (s *Service) joinRoomLocally(roomID, playerID, sessionID string, metadata map[string]string) (*Room, bool) {
	room, exists := s.rooms[roomID]
	if !exists {
		return nil, false
	}

	if len(room.Players) >= room.MaxPlayers && room.MaxPlayers > 0 {
		return nil, false
	}

	if existingRoomID, inRoom := s.playerRooms[playerID]; inRoom {
		if existingRoomID == roomID {
			return room, true
		}

		if existingRoom, roomExists := s.rooms[existingRoomID]; roomExists {
			delete(existingRoom.Players, playerID)
		}
	}

	room.Players[playerID] = &RoomPlayer{
		PlayerID:  playerID,
		SessionID: sessionID,
		IsReady:   false,
		JoinedAt:  time.Now(),
		Metadata:  metadata,
	}

	s.playerRooms[playerID] = roomID
	room.UpdatedAt = time.Now()

	s.log.Infow("player joined room locally (fallback)",
		"room_id", roomID,
		"player_id", playerID)

	return room, true
}

func (s *Service) LeaveRoom(playerID string) (*Room, bool) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	roomID, exists := s.playerRooms[playerID]
	if !exists {
		return nil, false
	}

	room, roomExists := s.rooms[roomID]
	if !roomExists {
		delete(s.playerRooms, playerID)
		return nil, false
	}

	// Call gRPC service to leave room
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	_, err := s.grpcClient.LeaveRoom(ctx, &proto.LeaveRoomRequest{
		RoomId:    roomID,
		UserId:    playerID,
		SessionId: room.Players[playerID].SessionID,
	})

	if err != nil {
		s.log.Errorw("failed to leave room via gRPC", "room_id", roomID, "player_id", playerID, "error", err)
		// Continue with local leave logic even if gRPC fails
	}

	// Update local state
	delete(room.Players, playerID)
	delete(s.playerRooms, playerID)

	if len(room.Players) == 0 {
		delete(s.rooms, roomID)
		s.log.Infow("room deleted (empty)", "room_id", roomID)
		return nil, true
	}

	if room.OwnerID == playerID && len(room.Players) > 0 {
		for pid := range room.Players {
			room.OwnerID = pid
			break
		}
		s.log.Infow("room owner changed",
			"room_id", roomID,
			"new_owner_id", room.OwnerID)
	}

	room.UpdatedAt = time.Now()

	s.log.Infow("player left room",
		"room_id", roomID,
		"player_id", playerID)

	return room, true
}

func (s *Service) GetPlayersInRoom(roomID string) []string {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	room, exists := s.rooms[roomID]
	if !exists {
		return []string{}
	}

	players := make([]string, 0, len(room.Players))
	for playerID := range room.Players {
		players = append(players, playerID)
	}

	return players
}

func (s *Service) DeleteRoom(roomID string) bool {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	room, exists := s.rooms[roomID]
	if !exists {
		return false
	}

	// Call gRPC service to delete room
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	_, err := s.grpcClient.DeleteRoom(ctx, &proto.DeleteRoomRequest{
		RoomId: roomID,
	})

	if err != nil {
		s.log.Errorw("failed to delete room via gRPC", "room_id", roomID, "error", err)
		// Continue with local delete logic even if gRPC fails
	}

	// Update local state
	for playerID := range room.Players {
		delete(s.playerRooms, playerID)
	}

	delete(s.rooms, roomID)

	s.log.Infow("room deleted", "room_id", roomID)

	return true
}

func (s *Service) SetPlayerReady(playerID string, isReady bool) bool {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	roomID, exists := s.playerRooms[playerID]
	if !exists {
		return false
	}

	room, roomExists := s.rooms[roomID]
	if !roomExists {
		return false
	}

	player, playerExists := room.Players[playerID]
	if !playerExists {
		return false
	}

	player.IsReady = isReady
	room.UpdatedAt = time.Now()

	// Update room state via gRPC if needed
	// This is optional as player readiness might be handled differently in your system
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	readyValue := "false"
	if isReady {
		readyValue = "true"
	}
	metadata := map[string]string{
		"player_" + playerID + "_ready": readyValue,
	}

	_, err := s.grpcClient.UpdateRoomState(ctx, &proto.UpdateRoomStateRequest{
		RoomId:   roomID,
		Metadata: metadata,
	})

	if err != nil {
		s.log.Warnw("failed to update room state via gRPC", "room_id", roomID, "error", err)
		// Continue with local state update even if gRPC fails
	}

	s.log.Infow("player ready status changed",
		"room_id", roomID,
		"player_id", playerID,
		"is_ready", isReady)

	return true
}

func (s *Service) AreAllPlayersReady(roomID string) bool {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	room, exists := s.rooms[roomID]
	if !exists || len(room.Players) == 0 {
		return false
	}

	for _, player := range room.Players {
		if !player.IsReady {
			return false
		}
	}

	return true
}

func (s *Service) PersistGameState(roomID string, stateData []byte, currentTurnUserID string, turnNumber int, metadata map[string]string) bool {
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	_, err := s.grpcClient.PersistGameState(ctx, &proto.PersistGameStateRequest{
		RoomId:            roomID,
		StateData:         stateData,
		CurrentTurnUserId: currentTurnUserID,
		TurnNumber:        int32(turnNumber),
		Metadata:          metadata,
	})

	if err != nil {
		s.log.Errorw("failed to persist game state via gRPC", "room_id", roomID, "error", err)
		return false
	}

	s.log.Infow("game state persisted", "room_id", roomID)
	return true
}

func (s *Service) UpdateRoomState(roomID string, state int, currentTurnUserID string, turnNumber int, metadata map[string]string) (*Room, bool) {
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	turnNumberPtr := int32(turnNumber)
	var currentTurnUserIdPtr *string
	if currentTurnUserID != "" {
		currentTurnUserIdPtr = &currentTurnUserID
	}

	protoRoom, err := s.grpcClient.UpdateRoomState(ctx, &proto.UpdateRoomStateRequest{
		RoomId:            roomID,
		State:             proto.RoomState(state),
		CurrentTurnUserId: currentTurnUserIdPtr,
		TurnNumber:        &turnNumberPtr,
		Metadata:          metadata,
	})

	if err != nil {
		s.log.Errorw("failed to update room state via gRPC", "room_id", roomID, "error", err)
		return nil, false
	}

	s.mutex.Lock()
	defer s.mutex.Unlock()

	room, exists := s.rooms[roomID]
	if !exists {
		room = &Room{
			ID:         protoRoom.Id,
			Name:       protoRoom.Id,
			GameType:   protoRoom.GameId,
			MaxPlayers: int(protoRoom.MaxPlayers),
			IsPrivate:  false,
			OwnerID:    protoRoom.CreatorId,
			Players:    make(map[string]*RoomPlayer),
			Metadata:   protoRoom.Metadata,
			CreatedAt:  protoRoom.CreatedAt.AsTime(),
			UpdatedAt:  time.Now(),
		}
		s.rooms[roomID] = room
	} else {
		room.Metadata = protoRoom.Metadata
		room.UpdatedAt = time.Now()
	}

	s.log.Infow("room state updated", "room_id", roomID, "state", state)
	return room, true
}

func (s *Service) KickPlayer(roomID string, playerID string, reason string) bool {
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	_, err := s.grpcClient.KickPlayer(ctx, &proto.KickPlayerRequest{
		RoomId: roomID,
		UserId: playerID,
		Reason: reason,
	})

	if err != nil {
		s.log.Errorw("failed to kick player via gRPC", "room_id", roomID, "player_id", playerID, "error", err)
		return false
	}

	s.mutex.Lock()
	defer s.mutex.Unlock()

	delete(s.playerRooms, playerID)

	if room, exists := s.rooms[roomID]; exists {
		delete(room.Players, playerID)
	}

	s.log.Infow("player kicked from room", "room_id", roomID, "player_id", playerID, "reason", reason)
	return true
}
