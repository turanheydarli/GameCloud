package main

import (
	"context"
	"flag"
	"fmt"
	"os"
	"os/signal"
	"syscall"

	"github.com/turanheydarli/gamecloud/relay/internal/foundation"
	"github.com/turanheydarli/gamecloud/relay/internal/foundation/config"
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
)

func main() {
	envPath := flag.String("env", "./.env", "Path to .env configuration file")
	flag.Parse()

	cfg, err := config.Load(*envPath)
	if err != nil {
		fmt.Printf("WARNING: Failed to load configuration file: %v\n", err)
		fmt.Println("Continuing with default configuration...")
		cfg = config.New()
	}

	log := logger.NewStdLogger(cfg.Log.Level)
	log.Infow("starting relay server", "port", cfg.Server.Port)

	app, err := foundation.NewApp(*cfg, log)
	if err != nil {
		log.Fatalw("failed to create application", "error", err)
	}

	shutdown := make(chan os.Signal, 1)
	signal.Notify(shutdown, syscall.SIGINT, syscall.SIGTERM)

	appErrors := make(chan error, 1)
	go func() {
		log.Infow("starting application")
		appErrors <- app.Start(context.Background())
	}()

	select {
	case err := <-appErrors:
		if err != nil {
			log.Errorw("application error", "error", err)
		}
	case sig := <-shutdown:
		log.Infow("shutdown signal received", "signal", sig)
	}

	log.Infow("stopping application...")
	if err := app.Stop(); err != nil {
		log.Errorw("error stopping application", "error", err)
	}

	log.Infow("application stopped")
}
