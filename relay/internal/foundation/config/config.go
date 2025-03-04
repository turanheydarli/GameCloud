package config

import (
	"context"
	"encoding/json"
	"fmt"
	"os"
	"path/filepath"
	"strconv"
	"strings"
	"time"

	"github.com/joho/godotenv"
)

type Config struct {
	Server struct {
		Port         int           `json:"port" env:"PORT"`
		ReadTimeout  time.Duration `json:"readTimeout" env:"READ_TIMEOUT"`
		WriteTimeout time.Duration `json:"writeTimeout" env:"WRITE_TIMEOUT"`
		ShutdownTime time.Duration `json:"shutdownTime" env:"SHUTDOWN_TIME"`
	} `json:"server"`

	Log struct {
		Level string `json:"level" env:"LOG_LEVEL"`
	} `json:"log"`

	GRPC struct {
		ServerAddr string        `json:"serverAddr" env:"GRPC_SERVER_ADDR"`
		Timeout    time.Duration `json:"timeout" env:"GRPC_TIMEOUT"`
	} `json:"grpc"`

	WebSocket struct {
		MaxMessageSize   int64         `json:"maxMessageSize" env:"WS_MAX_MESSAGE_SIZE"`
		WriteWait        time.Duration `json:"writeWait" env:"WS_WRITE_WAIT"`
		PongWait         time.Duration `json:"pongWait" env:"WS_PONG_WAIT"`
		PingPeriod       time.Duration `json:"pingPeriod" env:"WS_PING_PERIOD"`
		MaxMessageBuffer int           `json:"maxMessageBuffer" env:"WS_MAX_MESSAGE_BUFFER"`
		CloseGracePeriod time.Duration `json:"closeGracePeriod" env:"WS_CLOSE_GRACE_PERIOD"`
	} `json:"websocket"`
}

func New() *Config {
	cfg := Config{}

	cfg.Server.Port = 8080
	cfg.Server.ReadTimeout = 5 * time.Second
	cfg.Server.WriteTimeout = 10 * time.Second
	cfg.Server.ShutdownTime = 20 * time.Second

	cfg.Log.Level = "info"

	cfg.GRPC.ServerAddr = "localhost:5005"
	cfg.GRPC.Timeout = 10 * time.Second

	cfg.WebSocket.MaxMessageSize = 32 * 1024
	cfg.WebSocket.WriteWait = 10 * time.Second
	cfg.WebSocket.PongWait = 60 * time.Second
	cfg.WebSocket.PingPeriod = 45 * time.Second
	cfg.WebSocket.MaxMessageBuffer = 256
	cfg.WebSocket.CloseGracePeriod = 10 * time.Second

	return &cfg
}

func Load(configPath string) (*Config, error) {
	cfg := New()

	_ = godotenv.Load()

	if configPath != "" {
		if err := cfg.loadFromFile(configPath); err != nil {
			return nil, fmt.Errorf("loading config from file: %w", err)
		}
	}

	if err := cfg.loadFromEnv(); err != nil {
		return nil, fmt.Errorf("loading config from environment: %w", err)
	}

	if err := cfg.validate(); err != nil {
		return nil, fmt.Errorf("validating config: %w", err)
	}

	return cfg, nil
}

func (cfg *Config) loadFromFile(path string) error {
	absPath, err := filepath.Abs(path)
	if err != nil {
		return fmt.Errorf("getting absolute path: %w", err)
	}

	file, err := os.Open(absPath)
	if err != nil {
		return fmt.Errorf("opening config file: %w", err)
	}
	defer file.Close()

	decoder := json.NewDecoder(file)
	if err := decoder.Decode(cfg); err != nil {
		return fmt.Errorf("decoding config file: %w", err)
	}

	return nil
}

func (cfg *Config) loadFromEnv() error {
	if v := os.Getenv("PORT"); v != "" {
		if port, err := strconv.Atoi(v); err == nil {
			cfg.Server.Port = port
		}
	}

	if v := os.Getenv("READ_TIMEOUT"); v != "" {
		if d, err := time.ParseDuration(v); err == nil {
			cfg.Server.ReadTimeout = d
		}
	}

	if v := os.Getenv("WRITE_TIMEOUT"); v != "" {
		if d, err := time.ParseDuration(v); err == nil {
			cfg.Server.WriteTimeout = d
		}
	}

	if v := os.Getenv("SHUTDOWN_TIME"); v != "" {
		if d, err := time.ParseDuration(v); err == nil {
			cfg.Server.ShutdownTime = d
		}
	}

	if v := os.Getenv("LOG_LEVEL"); v != "" {
		cfg.Log.Level = v
	}

	if v := os.Getenv("GRPC_SERVER_ADDR"); v != "" {
		cfg.GRPC.ServerAddr = v
	}

	if v := os.Getenv("GRPC_TIMEOUT"); v != "" {
		if d, err := time.ParseDuration(v); err == nil {
			cfg.GRPC.Timeout = d
		}
	}

	if v := os.Getenv("WS_MAX_MESSAGE_SIZE"); v != "" {
		if size, err := strconv.ParseInt(v, 10, 64); err == nil {
			cfg.WebSocket.MaxMessageSize = size
		}
	}

	if v := os.Getenv("WS_WRITE_WAIT"); v != "" {
		if d, err := time.ParseDuration(v); err == nil {
			cfg.WebSocket.WriteWait = d
		}
	}

	if v := os.Getenv("WS_PONG_WAIT"); v != "" {
		if d, err := time.ParseDuration(v); err == nil {
			cfg.WebSocket.PongWait = d
		}
	}

	if v := os.Getenv("WS_PING_PERIOD"); v != "" {
		if d, err := time.ParseDuration(v); err == nil {
			cfg.WebSocket.PingPeriod = d
		}
	}

	if v := os.Getenv("WS_MAX_MESSAGE_BUFFER"); v != "" {
		if size, err := strconv.Atoi(v); err == nil {
			cfg.WebSocket.MaxMessageBuffer = size
		}
	}

	if v := os.Getenv("WS_CLOSE_GRACE_PERIOD"); v != "" {
		if d, err := time.ParseDuration(v); err == nil {
			cfg.WebSocket.CloseGracePeriod = d
		}
	}

	return nil
}

func (cfg *Config) validate() error {
	validLevels := map[string]bool{
		"debug": true,
		"info":  true,
		"warn":  true,
		"error": true,
		"fatal": true,
	}

	if !validLevels[strings.ToLower(cfg.Log.Level)] {
		return fmt.Errorf("invalid log level: %s", cfg.Log.Level)
	}

	if cfg.Server.Port < 1 || cfg.Server.Port > 65535 {
		return fmt.Errorf("invalid port: %d", cfg.Server.Port)
	}

	if cfg.WebSocket.PingPeriod >= cfg.WebSocket.PongWait {
		return fmt.Errorf("ping period must be less than pong wait")
	}

	return nil
}

func NewShutdownContext(cfg *Config) (context.Context, context.CancelFunc) {
	return context.WithTimeout(context.Background(), cfg.Server.ShutdownTime)
}
