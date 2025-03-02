package main

import (
	"flag"
	"fmt"
	"log"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/turanheydarli/gamecloud/relay/internal/foundation"
	"github.com/turanheydarli/gamecloud/relay/internal/matchmaking"
	"github.com/turanheydarli/gamecloud/relay/internal/rtapi"
)

var (
	port = flag.Int("port", 7001, "The port to run the server on")
)

func main() {
	flag.Parse()

	cfg, err := foundation.LoadConfig() 
	if err != nil {
		log.Fatalf("Error loading config: %v", err)
	}

	logger := foundation.NewLogger(cfg.LogLevel)

	logger.Infow("startup", "status", "initializing", "port", *port)

	wsManager := rtapi.NewWebSocketManager(logger)

	matchmakingService := matchmaking.NewService(logger, wsManager)

	rtapiHandler := rtapi.NewHandler(logger, wsManager, matchmakingService)

	mux := http.NewServeMux()

	mux.HandleFunc("/ws", func(w http.ResponseWriter, r *http.Request) {
		rtapiHandler.HandleWebSocket(w, r)
	})

	serverAddr := fmt.Sprintf(":%d", *port)
	server := &http.Server{
		Addr:         serverAddr,
		Handler:      mux,
		ReadTimeout:  5 * time.Second,
		WriteTimeout: 10 * time.Second,
	}

	go func() {
		logger.Infow("startup", "status", "listening", "addr", serverAddr)
		if err := server.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			logger.Fatalw("server failed", "error", err)
		}
	}()

	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit

	logger.Infow("shutdown", "status", "initiating graceful shutdown")

	shutdownCtx, cancel := foundation.NewShutdownContext(10 * time.Second)
	defer cancel()

	if err := server.Shutdown(shutdownCtx); err != nil {
		logger.Errorw("shutdown error", "error", err)
	}

	logger.Infow("shutdown", "status", "complete")
}
