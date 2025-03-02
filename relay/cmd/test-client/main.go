package main

import (
	"flag"
	"fmt"
	"log"
	"os"
	"os/signal"
	"time"

	"github.com/gorilla/websocket"
	pbrt "github.com/turanheydarli/gamecloud/relay/proto"
	"google.golang.org/protobuf/proto"
)

var addr = flag.String("addr", "localhost:7001", "WebSocket server address")
var playerID = flag.String("player", "test-player-1", "Player ID to use for connection")

func main() {
	flag.Parse()

	// Set up a channel to handle OS signals for graceful shutdown
	interrupt := make(chan os.Signal, 1)
	signal.Notify(interrupt, os.Interrupt)

	// Connect to the WebSocket server
	url := fmt.Sprintf("ws://%s/ws?player_id=%s", *addr, *playerID)
	log.Printf("Connecting to %s", url)

	conn, _, err := websocket.DefaultDialer.Dial(url, nil)
	if err != nil {
		log.Fatalf("Failed to connect to WebSocket server: %v", err)
	}
	defer conn.Close()

	log.Printf("Connected to WebSocket server")

	// Create a channel to receive WebSocket messages
	done := make(chan struct{})

	// Start a goroutine to read messages from the WebSocket
	go func() {
		defer close(done)
		for {
			_, message, err := conn.ReadMessage()
			if err != nil {
				log.Printf("Error reading from WebSocket: %v", err)
				return
			}

			// Unmarshal the message
			envelope := &pbrt.Envelope{}
			if err := proto.Unmarshal(message, envelope); err != nil {
				log.Printf("Error unmarshaling message: %v", err)
				continue
			}

			// Print the message
			log.Printf("Received message: %v", envelope)
		}
	}()

	// Send a test message (MatchmakerAdd)
	matchmakerAdd := &pbrt.Envelope{
		Id: "test-message-1",
		Message: &pbrt.Envelope_MatchmakerAdd{
			MatchmakerAdd: &pbrt.MatchmakerAdd{
				GameId:   "test-game",
				MinCount: 2,
				MaxCount: 4,
			},
		},
	}

	// Marshal the message
	data, err := proto.Marshal(matchmakerAdd)
	if err != nil {
		log.Fatalf("Error marshaling message: %v", err)
	}

	// Send the message
	if err := conn.WriteMessage(websocket.BinaryMessage, data); err != nil {
		log.Fatalf("Error sending message: %v", err)
	}
	log.Printf("Sent MatchmakerAdd message")

	// Wait for interrupt signal or done channel to close
	ticker := time.NewTicker(time.Second)
	defer ticker.Stop()

	for {
		select {
		case <-done:
			return
		case <-ticker.C:
			// Keep the connection alive
		case <-interrupt:
			log.Println("Interrupt received, closing connection...")

			// Cleanly close the connection
			err := conn.WriteMessage(websocket.CloseMessage, websocket.FormatCloseMessage(websocket.CloseNormalClosure, ""))
			if err != nil {
				log.Printf("Error during close: %v", err)
			}

			// Wait for the server to close the connection
			select {
			case <-done:
			case <-time.After(time.Second):
			}
			return
		}
	}
}
