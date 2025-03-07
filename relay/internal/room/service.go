package room

import (
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
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
}

func NewService(log logger.Logger) *Service {
	return &Service{
		log:         log,
		rooms:       make(map[string]*Room),
		playerRooms: make(map[string]string),
	}
}

func (s *Service) CreateRoom(name, gameType string, maxPlayers int, isPrivate bool,
	password string, ownerID string, metadata map[string]string) *Room {

	s.mutex.Lock()
	defer s.mutex.Unlock()

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

	s.log.Infow("room created",
		"room_id", roomID,
		"owner_id", ownerID,
		"game_type", gameType)

	return room
}

func (s *Service) GetRoom(roomID string) (*Room, bool) {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	room, exists := s.rooms[roomID]
	return room, exists
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

	s.log.Infow("player joined room",
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
