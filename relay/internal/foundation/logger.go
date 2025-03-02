package foundation

import (
	"github.com/turanheydarli/gamecloud/relay/pkg/logger"
)

// NewLogger creates a logger that meets your interface. 
func NewLogger(level string) logger.Logger {
	// In a real project, you might create a zap or zerolog logger.
	return logger.NewStdLogger(level) 
} 