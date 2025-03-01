package main

import (
	"flag"
	"fmt"
	"log"
	"net/http"
	"os"
	"os/signal"
	"syscall"

	"github.com/turanheydarli/gamecloud/relay/internal/transport"
)

var (
	port = flag.Int("port", 7001, "The port to run the server on")
)

func main() {
	flag.Parse()

	log.Printf("GameCloud Relay Server starting on port %d", *port)

	wsServer := transport.NewWebSocketServer()

	http.HandleFunc("/ws", wsServer.HandleConnection)

	serverAddr := fmt.Sprintf(":%d", *port)
	server := &http.Server{Addr: serverAddr}

	go func() {
		if err := server.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			log.Fatalf("Server error: %v", err)
		}
	}()

	log.Printf("GameCloud Relay Server started on port %d", *port)
	log.Println("Press Ctrl+C to stop the server")

	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit

	log.Println("Server shutting down...")
}
