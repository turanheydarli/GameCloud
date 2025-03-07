package transport

import (
	"context"

	"google.golang.org/grpc/metadata"
	"google.golang.org/protobuf/proto"
)

func CreateGRPCContext(ctx context.Context, gameKey string) context.Context {
	if gameKey == "" {
		return ctx
	}

	md := metadata.Pairs("X-Game-Key", gameKey)
	return metadata.NewOutgoingContext(ctx, md)
}

func MarshalProto(message proto.Message) ([]byte, error) {
	return proto.Marshal(message)
}

func UnmarshalProto(data []byte, message proto.Message) error {
	return proto.Unmarshal(data, message)
}
