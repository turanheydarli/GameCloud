FROM golang:1.21-alpine AS builder

WORKDIR /app

COPY go.mod go.sum ./

RUN go mod download

COPY . .

RUN CGO_ENABLED=0 GOOS=linux go build -o relay-server ./cmd/relay-server

FROM alpine:latest

WORKDIR /app

COPY --from=builder /app/relay-server .

RUN adduser -D appuser
USER appuser

EXPOSE 8090
EXPOSE 9000

ENTRYPOINT ["./relay-server"] 