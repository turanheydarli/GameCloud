package room

import (
	"sync"
	"time"

	"github.com/google/uuid"
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
)

// Room represents a multiplayer room
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

// RoomPlayer represents a player in a room
type RoomPlayer struct {
	PlayerID  string
	SessionID string
	IsReady   bool
	JoinedAt  time.Time
	Metadata  map[string]string
}

// Service manages room operations
type Service struct {
	log         logger.Logger
	rooms       map[string]*Room
	playerRooms map[string]string // PlayerID -> RoomID
	mutex       sync.RWMutex
}

// NewService creates a new room service
func NewService(log logger.Logger) *Service {
	return &Service{
		log:         log,
		rooms:       make(map[string]*Room),
		playerRooms: make(map[string]string),
	}
}

// CreateRoom creates a new room
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

	// Add owner as first player
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

// GetRoom retrieves a room by ID
func (s *Service) GetRoom(roomID string) (*Room, bool) {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	room, exists := s.rooms[roomID]
	return room, exists
}

// GetPlayerRoom gets the room a player is in
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

// JoinRoom adds a player to a room
func (s *Service) JoinRoom(roomID, playerID, sessionID string, metadata map[string]string) (*Room, bool) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	room, exists := s.rooms[roomID]
	if !exists {
		return nil, false
	}

	// Check if room is full
	if len(room.Players) >= room.MaxPlayers && room.MaxPlayers > 0 {
		return nil, false
	}

	// Check if player is already in a room
	if existingRoomID, inRoom := s.playerRooms[playerID]; inRoom {
		if existingRoomID == roomID {
			// Already in this room
			return room, true
		}

		// Leave the current room first
		if existingRoom, roomExists := s.rooms[existingRoomID]; roomExists {
			delete(existingRoom.Players, playerID)
		}
	}

	// Add player to room
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

// LeaveRoom removes a player from a room
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

	// Remove player from room
	delete(room.Players, playerID)
	delete(s.playerRooms, playerID)

	// If room is empty, delete it
	if len(room.Players) == 0 {
		delete(s.rooms, roomID)
		s.log.Infow("room deleted (empty)", "room_id", roomID)
		return nil, true
	}

	// If owner left, assign new owner
	if room.OwnerID == playerID && len(room.Players) > 0 {
		// Pick first player as new owner
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

// GetPlayersInRoom gets all player IDs in a room
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

// DeleteRoom removes a room
func (s *Service) DeleteRoom(roomID string) bool {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	room, exists := s.rooms[roomID]
	if !exists {
		return false
	}

	// Remove all players from the room
	for playerID := range room.Players {
		delete(s.playerRooms, playerID)
	}

	delete(s.rooms, roomID)

	s.log.Infow("room deleted", "room_id", roomID)

	return true
}

// SetPlayerReady sets a player's ready status
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

// AreAllPlayersReady checks if all players in a room are ready
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
