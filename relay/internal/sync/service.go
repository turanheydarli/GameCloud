package sync

import (
	"sync"
	"time"

	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
)

type NetworkObject struct {
	ViewID      int32
	OwnerID     string
	RoomID      string
	PrefabName  string
	Position    *pbrt.Vector3
	Rotation    *pbrt.Quaternion
	Scale       *pbrt.Vector3
	Properties  map[string][]byte
	CreatedAt   time.Time
	LastUpdated time.Time
}

type Service struct {
	log           logger.Logger
	objects       map[int32]*NetworkObject
	roomObjects   map[string]map[int32]*NetworkObject
	playerObjects map[string]map[int32]*NetworkObject
	nextViewID    int32
	mutex         sync.RWMutex
}

func NewService(log logger.Logger) *Service {
	return &Service{
		log:           log,
		objects:       make(map[int32]*NetworkObject),
		roomObjects:   make(map[string]map[int32]*NetworkObject),
		playerObjects: make(map[string]map[int32]*NetworkObject),
		nextViewID:    1,
	}
}

func (s *Service) CreateObject(ownerID, roomID, prefabName string, position *pbrt.Vector3,
	rotation *pbrt.Quaternion, scale *pbrt.Vector3, properties map[string][]byte) *NetworkObject {

	s.mutex.Lock()
	defer s.mutex.Unlock()

	viewID := s.nextViewID
	s.nextViewID++

	now := time.Now()

	obj := &NetworkObject{
		ViewID:      viewID,
		OwnerID:     ownerID,
		RoomID:      roomID,
		PrefabName:  prefabName,
		Position:    position,
		Rotation:    rotation,
		Scale:       scale,
		Properties:  properties,
		CreatedAt:   now,
		LastUpdated: now,
	}

	s.objects[viewID] = obj

	if _, exists := s.roomObjects[roomID]; !exists {
		s.roomObjects[roomID] = make(map[int32]*NetworkObject)
	}
	s.roomObjects[roomID][viewID] = obj

	if _, exists := s.playerObjects[ownerID]; !exists {
		s.playerObjects[ownerID] = make(map[int32]*NetworkObject)
	}
	s.playerObjects[ownerID][viewID] = obj

	s.log.Infow("object created",
		"view_id", viewID,
		"owner_id", ownerID,
		"room_id", roomID,
		"prefab", prefabName)

	return obj
}

func (s *Service) GetObject(viewID int32) (*NetworkObject, bool) {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	obj, exists := s.objects[viewID]
	return obj, exists
}

func (s *Service) GetRoomObjects(roomID string) map[int32]*NetworkObject {
	s.mutex.RLock()
	defer s.mutex.RUnlock()

	if roomObjs, exists := s.roomObjects[roomID]; exists {
		// Create a copy to avoid concurrent map access
		result := make(map[int32]*NetworkObject, len(roomObjs))
		for id, obj := range roomObjs {
			result[id] = obj
		}
		return result
	}

	return make(map[int32]*NetworkObject)
}

func (s *Service) UpdateObjectTransform(viewID int32, position *pbrt.Vector3,
	rotation *pbrt.Quaternion, scale *pbrt.Vector3) (*NetworkObject, bool) {

	s.mutex.Lock()
	defer s.mutex.Unlock()

	obj, exists := s.objects[viewID]
	if !exists {
		return nil, false
	}

	if position != nil {
		obj.Position = position
	}

	if rotation != nil {
		obj.Rotation = rotation
	}

	if scale != nil {
		obj.Scale = scale
	}

	obj.LastUpdated = time.Now()

	s.log.Infow("object transform updated",
		"view_id", viewID,
		"owner_id", obj.OwnerID,
		"room_id", obj.RoomID)

	return obj, true
}

func (s *Service) UpdateObjectProperties(viewID int32, properties map[string][]byte) (*NetworkObject, bool) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	obj, exists := s.objects[viewID]
	if !exists {
		return nil, false
	}

	for key, value := range properties {
		obj.Properties[key] = value
	}

	obj.LastUpdated = time.Now()

	s.log.Infow("object properties updated",
		"view_id", viewID,
		"owner_id", obj.OwnerID,
		"room_id", obj.RoomID)

	return obj, true
}

func (s *Service) DeleteObject(viewID int32) (string, bool) {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	obj, exists := s.objects[viewID]
	if !exists {
		return "", false
	}

	roomID := obj.RoomID
	ownerID := obj.OwnerID

	if roomObjs, exists := s.roomObjects[roomID]; exists {
		delete(roomObjs, viewID)
	}

	if playerObjs, exists := s.playerObjects[ownerID]; exists {
		delete(playerObjs, viewID)
	}

	delete(s.objects, viewID)

	s.log.Infow("object deleted",
		"view_id", viewID,
		"owner_id", ownerID,
		"room_id", roomID)

	return roomID, true
}

func (s *Service) CleanupRoomObjects(roomID string) int {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	count := 0

	if roomObjs, exists := s.roomObjects[roomID]; exists {
		for viewID, obj := range roomObjs {
			if playerObjs, playerExists := s.playerObjects[obj.OwnerID]; playerExists {
				delete(playerObjs, viewID)
			}

			delete(s.objects, viewID)
			count++
		}

		delete(s.roomObjects, roomID)
	}

	if count > 0 {
		s.log.Infow("cleaned up room objects",
			"room_id", roomID,
			"count", count)
	}

	return count
}

func (s *Service) CleanupPlayerObjects(playerID string) int {
	s.mutex.Lock()
	defer s.mutex.Unlock()

	count := 0

	if playerObjs, exists := s.playerObjects[playerID]; exists {
		for viewID, obj := range playerObjs {
			if roomObjs, roomExists := s.roomObjects[obj.RoomID]; roomExists {
				delete(roomObjs, viewID)
			}

			delete(s.objects, viewID)
			count++
		}

		delete(s.playerObjects, playerID)
	}

	if count > 0 {
		s.log.Infow("cleaned up player objects",
			"player_id", playerID,
			"count", count)
	}

	return count
}
