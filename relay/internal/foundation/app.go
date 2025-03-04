package foundation

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"time"

	"github.com/turanheydarli/gamecloud/relay/internal/foundation/config"
	"github.com/turanheydarli/gamecloud/relay/internal/player"
	"github.com/turanheydarli/gamecloud/relay/internal/rtapi"
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
	"google.golang.org/grpc"
	"google.golang.org/grpc/connectivity"
	"google.golang.org/grpc/credentials/insecure"
)

type App struct {
	Config     config.Config
	Logger     logger.Logger
	HttpServer *http.Server
	GrpcConn   *grpc.ClientConn
}

func NewApp(cfg config.Config, log logger.Logger) (*App, error) {
	app := &App{
		Config: cfg,
		Logger: log,
	}

	log.Infow("connecting to gRPC server", "address", cfg.GRPC.ServerAddr)
	conn, err := grpc.Dial(
		cfg.GRPC.ServerAddr,
		grpc.WithTransportCredentials(insecure.NewCredentials()),
	)
	if err != nil {
		return nil, fmt.Errorf("failed to create gRPC client: %w", err)
	}
	app.GrpcConn = conn

	app.HttpServer = &http.Server{
		Addr:         fmt.Sprintf(":%d", cfg.Server.Port),
		Handler:      app.setupRoutes(),
		ReadTimeout:  cfg.Server.ReadTimeout,
		WriteTimeout: cfg.Server.WriteTimeout,
	}

	return app, nil
}

func (a *App) Start(ctx context.Context) error {
	go a.monitorGrpcConnection(ctx)

	a.Logger.Infow("starting HTTP server", "port", a.Config.Server.Port)
	if err := a.HttpServer.ListenAndServe(); err != nil && err != http.ErrServerClosed {
		return fmt.Errorf("HTTP server error: %w", err)
	}

	return nil
}

func (a *App) monitorGrpcConnection(ctx context.Context) {
	ticker := time.NewTicker(30 * time.Second)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			return
		case <-ticker.C:
			state := a.GrpcConn.GetState()

			if state == connectivity.TransientFailure || state == connectivity.Shutdown {
				a.Logger.Warnw("gRPC connection is in a bad state, attempting to reconnect", "state", state)
				a.GrpcConn.ResetConnectBackoff()
			}
		}
	}
}

// Stop gracefully stops the application
func (a *App) Stop() error {
	a.Logger.Infow("shutdown", "status", "closing resources")

	if a.GrpcConn != nil {
		if err := a.GrpcConn.Close(); err != nil {
			return fmt.Errorf("error closing gRPC connection: %w", err)
		}
	}

	return nil
}

func (a *App) setupRoutes() http.Handler {
	mux := http.NewServeMux()

	handler := a.buildHandler()

	mux.HandleFunc("/ws", handler.HandleWebSocket)
	mux.HandleFunc("/health", func(w http.ResponseWriter, r *http.Request) {
		state := a.GrpcConn.GetState()

		w.Header().Set("Content-Type", "application/json")

		type healthResponse struct {
			Status    string `json:"status"`
			GrpcState string `json:"grpc_state"`
			Timestamp string `json:"timestamp"`
			Version   string `json:"version"`
		}

		status := "healthy"
		if state != connectivity.Ready && state != connectivity.Idle {
			status = "degraded"
			w.WriteHeader(http.StatusServiceUnavailable)
		} else {
			w.WriteHeader(http.StatusOK)
		}

		resp := healthResponse{
			Status:    status,
			GrpcState: state.String(),
			Timestamp: time.Now().Format(time.RFC3339),
			Version:   "1.0.0",
		}

		if err := json.NewEncoder(w).Encode(resp); err != nil {
			a.Logger.Errorw("health check response encoding failed", "error", err)
		}
	})

	mux.HandleFunc("/ready", func(w http.ResponseWriter, r *http.Request) {
		state := a.GrpcConn.GetState()
		w.Header().Set("Content-Type", "application/json")

		type readyResponse struct {
			Ready     bool   `json:"ready"`
			GrpcState string `json:"grpc_state"`
			Message   string `json:"message"`
		}

		resp := readyResponse{
			GrpcState: state.String(),
		}

		if state == connectivity.Ready || state == connectivity.Idle {
			resp.Ready = true
			resp.Message = "Service is ready"
			w.WriteHeader(http.StatusOK)
		} else if state == connectivity.Connecting {
			resp.Ready = false
			resp.Message = "Service is initializing - gRPC connection is being established"
			w.WriteHeader(http.StatusServiceUnavailable)
		} else {
			resp.Ready = false
			resp.Message = fmt.Sprintf("Service is not ready - gRPC connection is in %s state", state)
			w.WriteHeader(http.StatusServiceUnavailable)

			// Try to reconnect if in a bad state
			if state == connectivity.TransientFailure || state == connectivity.Shutdown {
				a.Logger.Warnw("attempting to reconnect to gRPC server", "current_state", state)
				a.GrpcConn.ResetConnectBackoff()
			}
		}

		if err := json.NewEncoder(w).Encode(resp); err != nil {
			a.Logger.Errorw("ready check response encoding failed", "error", err)
		}
	})

	mux.HandleFunc("/live", func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusOK)
		w.Write([]byte("Alive"))
	})

	return mux
}

func (a *App) buildHandler() *rtapi.Handler {
	playerService := player.NewService(a.Logger, a.GrpcConn)

	handler := rtapi.NewHandler(
		a.Logger,
		playerService,
	)

	return handler
}
