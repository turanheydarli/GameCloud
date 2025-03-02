package foundation

import (
	"context"
	"time"
)

type Config struct {
	LogLevel string
}

// LoadConfig is a placeholder for real config loading logic.
func LoadConfig() (Config, error) {
	// For simplicity, return defaults.
	return Config{
		LogLevel: "debug",
	}, nil
}

// NewShutdownContext creates a context with the given timeout for graceful shutdown.
func NewShutdownContext(timeout time.Duration) (context.Context, context.CancelFunc) {
	return context.WithTimeout(context.Background(), timeout)
} 