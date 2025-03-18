FROM golang:1.21-alpine AS builder

WORKDIR /app

COPY go.mod go.sum ./

RUN go mod download

COPY . .

RUN CGO_ENABLED=0 GOOS=linux go build -o relay-server ./cmd/relay-server

FROM alpine:latest

WORKDIR /app

COPY --from=builder /app/relay-server .

RUN mkdir -p /app/config

RUN apk add --no-cache ca-certificates tzdata

RUN apk add --no-cache curl busybox-extras

ENV TZ=UTC

EXPOSE 8080

ENTRYPOINT ["./relay-server"] 