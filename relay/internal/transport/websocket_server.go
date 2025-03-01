package transport

import (
	"log"
	"net/http"
	"sync"

	"github.com/gorilla/websocket"
)

type WebSocketServer struct {
	upgrader    websocket.Upgrader
	connections map[string]*Connection
	mutex       sync.RWMutex
}

type Connection struct {
	ID        string
	Socket    *websocket.Conn
	SendChan  chan []byte
	CloseChan chan struct{}
}

func NewWebSocketServer() *WebSocketServer {
	return &WebSocketServer{
		upgrader: websocket.Upgrader{
			CheckOrigin: func(r *http.Request) bool {
				return true
			},
		},
		connections: make(map[string]*Connection),
	}
}

func (s *WebSocketServer) HandleConnection(w http.ResponseWriter, r *http.Request) {
	conn, err := s.upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Printf("Failed to upgrade connection: %v", err)
		return
	}

	connID := generateID()

	c := &Connection{
		ID:        connID,
		Socket:    conn,
		SendChan:  make(chan []byte, 256),
		CloseChan: make(chan struct{}),
	}

	s.mutex.Lock()
	s.connections[connID] = c
	s.mutex.Unlock()

	log.Printf("New connection established: %s", connID)

	go s.readPump(c)
	go s.writePump(c)
}

func (s *WebSocketServer) readPump(c *Connection) {
	defer func() {
		s.closeConnection(c)
	}()

	for {
		_, message, err := c.Socket.ReadMessage()
		if err != nil {
			if websocket.IsUnexpectedCloseError(err, websocket.CloseGoingAway, websocket.CloseAbnormalClosure) {
				log.Printf("Error reading from socket: %v", err)
			}
			break
		}

		log.Printf("Received message from %s: %s", c.ID, string(message))

		c.SendChan <- message
	}
}

func (s *WebSocketServer) writePump(c *Connection) {
	defer func() {
		s.closeConnection(c)
	}()

	for {
		select {
		case message, ok := <-c.SendChan:
			if !ok {
				c.Socket.WriteMessage(websocket.CloseMessage, []byte{})
				return
			}

			err := c.Socket.WriteMessage(websocket.TextMessage, message)
			if err != nil {
				log.Printf("Error writing to socket: %v", err)
				return
			}

		case <-c.CloseChan:
			return
		}
	}
}

func (s *WebSocketServer) closeConnection(c *Connection) {
	s.mutex.Lock()
	delete(s.connections, c.ID)
	s.mutex.Unlock()

	close(c.SendChan)
	c.Socket.Close()
	close(c.CloseChan)

	log.Printf("Connection closed: %s", c.ID)
}

func generateID() string {
	return "conn_" + randomString(8)
}

func randomString(length int) string {
	const charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
	b := make([]byte, length)
	for i := range b {
		b[i] = charset[i%len(charset)]
	}
	return string(b)
}
