package room_test

import (
	"context"
	"fmt"
	"os"
	"testing"
	"time"

	"github.com/google/uuid"
	"github.com/turanheydarli/gamecloud/relay/internal/room"
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

// TestMain handles setup and teardown for all tests
func TestMain(m *testing.M) {
	// Setup code - ensure the gRPC server is running
	fmt.Println("Setting up integration tests...")

	// Run tests
	exitCode := m.Run()

	// Teardown code
	fmt.Println("Cleaning up after integration tests...")

	os.Exit(exitCode)
}

func setupIntegrationService(t *testing.T) *room.Service {
	t.Helper()

	grpcAddr := os.Getenv("GRPC_SERVER_ADDR")
	if grpcAddr == "" {
		grpcAddr = "localhost:5005"
	}

	log := logger.NewStdLogger("test")

	service := room.NewService(log)

	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	conn, err := grpc.DialContext(ctx, grpcAddr,
		grpc.WithTransportCredentials(insecure.NewCredentials()),
		grpc.WithBlock())

	if err != nil {
		t.Fatalf("Failed to connect to gRPC server at %s: %v", grpcAddr, err)
	}

	service.Initialize(conn)

	t.Cleanup(func() {
		conn.Close()
	})

	return service
}

func generateTestID() string {
	return fmt.Sprintf("test-%s", uuid.New().String()[:8])
}

func TestRoomCreation(t *testing.T) {
	if testing.Short() {
		t.Skip("Skipping integration test in short mode")
	}

	svc := setupIntegrationService(t)

	roomName := fmt.Sprintf("test-room-%s", generateTestID())
	gameType := "chess"
	maxPlayers := 4
	isPrivate := false
	password := ""
	ownerID := uuid.New().String()
	metadata := map[string]string{"test": "integration"}

	createdRoom := svc.CreateRoom(roomName, gameType, maxPlayers, isPrivate, password, ownerID, metadata)

	if createdRoom == nil {
		t.Fatal("Room should not be nil")
	}
	if createdRoom.Name != roomName {
		t.Errorf("Expected room name %s, got %s", roomName, createdRoom.Name)
	}
	if createdRoom.GameType != gameType {
		t.Errorf("Expected game type %s, got %s", gameType, createdRoom.GameType)
	}

	// Clean up
	success := svc.DeleteRoom(createdRoom.ID)
	if !success {
		t.Error("Room deletion should succeed")
	}
}

func TestIntegrationGetRoom(t *testing.T) {
	// Skip in short mode
	if testing.Short() {
		t.Skip("Skipping integration test in short mode")
	}

	// Arrange
	svc := setupIntegrationService(t)

	// First create a room
	roomName := fmt.Sprintf("test-room-%s", generateTestID())
	gameType := "chess"
	ownerID := uuid.New().String()
	metadata := map[string]string{"test": "get-room"}

	createdRoom := svc.CreateRoom(roomName, gameType, 4, false, "", ownerID, metadata)
	if createdRoom == nil {
		t.Fatal("Room creation should succeed")
	}

	// Act
	fetchedRoom, exists := svc.GetRoom(createdRoom.ID)

	// Assert
	if !exists {
		t.Error("Room should exist")
	}
	if fetchedRoom == nil {
		t.Fatal("Fetched room should not be nil")
	}
	if fetchedRoom.ID != createdRoom.ID {
		t.Errorf("Room ID should match. Expected %s, got %s", createdRoom.ID, fetchedRoom.ID)
	}
	if fetchedRoom.Name != roomName {
		t.Errorf("Room name should match. Expected %s, got %s", roomName, fetchedRoom.Name)
	}
	if fetchedRoom.GameType != gameType {
		t.Errorf("Game type should match. Expected %s, got %s", gameType, fetchedRoom.GameType)
	}
	if fetchedRoom.OwnerID != ownerID {
		t.Errorf("Owner ID should match. Expected %s, got %s", ownerID, fetchedRoom.OwnerID)
	}
	if fetchedRoom.Metadata["test"] != "get-room" {
		t.Errorf("Metadata should match. Expected get-room, got %s", fetchedRoom.Metadata["test"])
	}

	// Clean up
	success := svc.DeleteRoom(createdRoom.ID)
	if !success {
		t.Error("Room deletion should succeed")
	}
}

func TestIntegrationJoinRoom(t *testing.T) {
	// Skip in short mode
	if testing.Short() {
		t.Skip("Skipping integration test in short mode")
	}

	// Arrange
	svc := setupIntegrationService(t)

	// First create a room
	roomName := fmt.Sprintf("test-room-%s", generateTestID())
	gameType := "chess"
	ownerID := uuid.New().String()

	createdRoom := svc.CreateRoom(roomName, gameType, 4, false, "", ownerID, nil)
	if createdRoom == nil {
		t.Fatal("Room creation should succeed")
	}

	// Create a new player to join the room
	playerID := uuid.New().String()
	sessionID := fmt.Sprintf("session-%s", generateTestID())
	playerMetadata := map[string]string{"role": "guest"}

	// Act
	_, success := svc.JoinRoom(createdRoom.ID, playerID, sessionID, playerMetadata)

	// Assert
	if !success {
		t.Error("Join room should succeed")
	}

	// Clean up
	success = svc.DeleteRoom(createdRoom.ID)
	if !success {
		t.Error("Room deletion should succeed")
	}
}

func TestIntegrationLeaveRoom(t *testing.T) {
	// Skip in short mode
	if testing.Short() {
		t.Skip("Skipping integration test in short mode")
	}

	// Arrange
	svc := setupIntegrationService(t)

	// First create a room
	roomName := fmt.Sprintf("test-room-%s", generateTestID())
	gameType := "chess"
	ownerID := uuid.New().String()

	createdRoom := svc.CreateRoom(roomName, gameType, 4, false, "", ownerID, nil)
	if createdRoom == nil {
		t.Fatal("Room creation should succeed")
	}

	// Add a second player
	playerID := uuid.New().String()
	sessionID := fmt.Sprintf("session-%s", generateTestID())

	joinedRoom, success := svc.JoinRoom(createdRoom.ID, playerID, sessionID, nil)
	if !success {
		t.Error("Join room should succeed")
	}
	if joinedRoom == nil {
		t.Error("Joined room should not be nil")
	}
	if !containsPlayer(joinedRoom.Players, playerID) {
		t.Error("Player should be in the room")
	}

	// Act - have the second player leave
	resultRoom, success := svc.LeaveRoom(playerID)

	// Assert
	if !success {
		t.Error("Leave room should succeed")
	}
	if resultRoom == nil {
		t.Error("Result room should not be nil")
	}
	if resultRoom.ID != createdRoom.ID {
		t.Errorf("Room ID should match. Expected %s, got %s", createdRoom.ID, resultRoom.ID)
	}
	if containsPlayer(resultRoom.Players, playerID) {
		t.Error("Player should no longer be in the room")
	}
	if !containsPlayer(resultRoom.Players, ownerID) {
		t.Error("Owner should still be in the room")
	}

	// Clean up
	success = svc.DeleteRoom(createdRoom.ID)
	if !success {
		t.Error("Room deletion should succeed")
	}
}

func TestIntegrationUpdateRoomState(t *testing.T) {
	// Skip in short mode
	if testing.Short() {
		t.Skip("Skipping integration test in short mode")
	}

	// Arrange
	svc := setupIntegrationService(t)

	// First create a room
	roomName := fmt.Sprintf("test-room-%s", generateTestID())
	gameType := "chess"
	ownerID := uuid.New().String()

	createdRoom := svc.CreateRoom(roomName, gameType, 4, false, "", ownerID, nil)
	if createdRoom == nil {
		t.Fatal("Room creation should succeed")
	}

	// Add a second player
	playerID := uuid.New().String()
	sessionID := fmt.Sprintf("session-%s", generateTestID())

	_, success := svc.JoinRoom(createdRoom.ID, playerID, sessionID, nil)
	if !success {
		t.Error("Join room should succeed")
	}

	// Act - update room state to started and set current turn
	newState := int(2) // ROOM_STATE_STARTED
	currentTurnUserID := playerID
	turnNumber := 1
	metadata := map[string]string{"gamePhase": "opening"}

	updatedRoom, success := svc.UpdateRoomState(createdRoom.ID, newState, currentTurnUserID, turnNumber, metadata)

	// Assert
	if !success {
		t.Error("Update room state should succeed")
	}
	if updatedRoom == nil {
		t.Error("Updated room should not be nil")
	}
	if updatedRoom.ID != createdRoom.ID {
		t.Errorf("Room ID should match. Expected %s, got %s", createdRoom.ID, updatedRoom.ID)
	}

	if updatedRoom.Metadata["gamePhase"] != "opening" {
		t.Errorf("Metadata should be updated. Expected opening, got %s", updatedRoom.Metadata["gamePhase"])
	}

	// Clean up
	success = svc.DeleteRoom(createdRoom.ID)
	if !success {
		t.Error("Room deletion should succeed")
	}
}

func TestIntegrationKickPlayer(t *testing.T) {
	// Skip in short mode
	if testing.Short() {
		t.Skip("Skipping integration test in short mode")
	}

	// Arrange
	svc := setupIntegrationService(t)

	// First create a room
	roomName := fmt.Sprintf("test-room-%s", generateTestID())
	gameType := "chess"
	ownerID := uuid.New().String()

	createdRoom := svc.CreateRoom(roomName, gameType, 4, false, "", ownerID, nil)
	if createdRoom == nil {
		t.Fatal("Room creation should succeed")
	}

	// Add a second player
	playerID := uuid.New().String()
	sessionID := fmt.Sprintf("session-%s", generateTestID())

	joinedRoom, success := svc.JoinRoom(createdRoom.ID, playerID, sessionID, nil)
	if !success {
		t.Error("Join room should succeed")
	}
	if !containsPlayer(joinedRoom.Players, playerID) {
		t.Error("Player should be in the room")
	}

	// Act - kick the player
	success = svc.KickPlayer(createdRoom.ID, playerID, "Inappropriate behavior")

	// Assert
	if !success {
		t.Error("Kick player should succeed")
	}

	// Verify player is no longer in the room
	room, exists := svc.GetRoom(createdRoom.ID)
	if !exists {
		t.Error("Room should still exist")
	}
	if containsPlayer(room.Players, playerID) {
		t.Error("Kicked player should no longer be in the room")
	}

	// Clean up
	success = svc.DeleteRoom(createdRoom.ID)
	if !success {
		t.Error("Room deletion should succeed")
	}
}

func TestIntegrationPersistGameState(t *testing.T) {
	// Skip in short mode
	if testing.Short() {
		t.Skip("Skipping integration test in short mode")
	}

	// Arrange
	svc := setupIntegrationService(t)

	// First create a room
	roomName := fmt.Sprintf("test-room-%s", generateTestID())
	gameType := "chess"
	ownerID := uuid.New().String()

	createdRoom := svc.CreateRoom(roomName, gameType, 4, false, "", ownerID, nil)
	if createdRoom == nil {
		t.Fatal("Room creation should succeed")
	}

	// Prepare game state data
	stateData := []byte(`{"board":{"a1":"wR","b1":"wN","c1":"wB","d1":"wQ","e1":"wK","f1":"wB","g1":"wN","h1":"wR"}}`)
	currentTurnUserID := ownerID
	turnNumber := 1
	metadata := map[string]string{
		"lastMove": "e2-e4",
		"phase":    "opening",
	}

	// Act
	success := svc.PersistGameState(createdRoom.ID, stateData, currentTurnUserID, turnNumber, metadata)

	// Assert
	if !success {
		t.Error("Persist game state should succeed")
	}

	// Verify state was persisted by checking room metadata
	room, exists := svc.GetRoom(createdRoom.ID)
	if !exists {
		t.Error("Room should still exist")
	}
	if room.Metadata["lastMove"] != "e2-e4" {
		t.Errorf("Room metadata should be updated. Expected e2-e4, got %s", room.Metadata["lastMove"])
	}
	if room.Metadata["phase"] != "opening" {
		t.Errorf("Room metadata should be updated. Expected opening, got %s", room.Metadata["phase"])
	}

	// Clean up
	success = svc.DeleteRoom(createdRoom.ID)
	if !success {
		t.Error("Room deletion should succeed")
	}
}

func TestIntegrationRoomLifecycle(t *testing.T) {
	// Skip in short mode
	if testing.Short() {
		t.Skip("Skipping integration test in short mode")
	}

	// This test covers the complete lifecycle of a room

	// Arrange
	svc := setupIntegrationService(t)

	// 1. Create a room
	roomName := fmt.Sprintf("lifecycle-room-%s", generateTestID())
	gameType := "chess"
	ownerID := uuid.New().String()

	room := svc.CreateRoom(roomName, gameType, 4, false, "", ownerID, nil)
	if room == nil {
		t.Fatal("Room creation should succeed")
	}

	// 2. Add players
	player1ID := uuid.New().String()
	player2ID := uuid.New().String()

	room, success := svc.JoinRoom(room.ID, player1ID, "session-1", nil)
	if !success {
		t.Error("Player 1 should join successfully")
	}

	room, success = svc.JoinRoom(room.ID, player2ID, "session-2", nil)
	if !success {
		t.Error("Player 2 should join successfully")
	}

	// 3. Set players ready
	success = svc.SetPlayerReady(ownerID, true)
	if !success {
		t.Error("Owner should be set ready")
	}

	success = svc.SetPlayerReady(player1ID, true)
	if !success {
		t.Error("Player 1 should be set ready")
	}

	success = svc.SetPlayerReady(player2ID, true)
	if !success {
		t.Error("Player 2 should be set ready")
	}

	// 4. Check all players ready
	allReady := svc.AreAllPlayersReady(room.ID)
	if !allReady {
		t.Error("All players should be ready")
	}

	// 5. Start the game
	room, success = svc.UpdateRoomState(room.ID, 2, ownerID, 1, map[string]string{"status": "started"})
	if !success {
		t.Error("Game should start successfully")
	}

	// 6. Make some moves
	success = svc.PersistGameState(room.ID, []byte(`{"move":"e2-e4"}`), player1ID, 2,
		map[string]string{"lastMove": "e2-e4"})
	if !success {
		t.Error("First move should be persisted")
	}

	success = svc.PersistGameState(room.ID, []byte(`{"move":"e7-e5"}`), player2ID, 3,
		map[string]string{"lastMove": "e7-e5"})
	if !success {
		t.Error("Second move should be persisted")
	}

	// 7. Player leaves
	room, success = svc.LeaveRoom(player2ID)
	if !success {
		t.Error("Player 2 should leave successfully")
	}
	if !containsPlayer(room.Players, player2ID) {
		t.Error("Player 2 should no longer be in the room")
	}

	// 8. End the game
	room, success = svc.UpdateRoomState(room.ID, 3, "", 0, map[string]string{"status": "ended", "winner": player1ID})
	if !success {
		t.Error("Game should end successfully")
	}

	// 9. Delete the room
	success = svc.DeleteRoom(room.ID)
	if !success {
		t.Error("Room should be deleted successfully")
	}

	// 10. Verify room no longer exists
	_, exists := svc.GetRoom(room.ID)
	if exists {
		t.Error("Room should no longer exist")
	}
}

func containsPlayer(players map[string]*room.RoomPlayer, playerID string) bool {
	_, exists := players[playerID]
	return exists
}
