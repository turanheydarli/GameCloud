// Code generated by protoc-gen-go-grpc. DO NOT EDIT.
// versions:
// - protoc-gen-go-grpc v1.2.0
// - protoc             v5.29.3
// source: gamecloud.proto

package proto

import (
	context "context"
	grpc "google.golang.org/grpc"
	codes "google.golang.org/grpc/codes"
	status "google.golang.org/grpc/status"
	emptypb "google.golang.org/protobuf/types/known/emptypb"
)

// This is a compile-time assertion to ensure that this generated file
// is compatible with the grpc package it is being compiled against.
// Requires gRPC-Go v1.32.0 or later.
const _ = grpc.SupportPackageIsVersion7

// RelayServiceClient is the client API for RelayService service.
//
// For semantics around ctx use and closing/ending streaming RPCs, please refer to https://pkg.go.dev/google.golang.org/grpc/?tab=doc#ClientConn.NewStream.
type RelayServiceClient interface {
	AuthenticateUser(ctx context.Context, in *AuthenticateRequest, opts ...grpc.CallOption) (*AuthenticateResponse, error)
	UpdatePlayer(ctx context.Context, in *UpdatePlayerRequest, opts ...grpc.CallOption) (*UpdatePlayerResponse, error)
	CreateRoom(ctx context.Context, in *CreateRoomRequest, opts ...grpc.CallOption) (*Room, error)
	GetRoom(ctx context.Context, in *GetRoomRequest, opts ...grpc.CallOption) (*Room, error)
	DeleteRoom(ctx context.Context, in *DeleteRoomRequest, opts ...grpc.CallOption) (*emptypb.Empty, error)
	UpdateRoomState(ctx context.Context, in *UpdateRoomStateRequest, opts ...grpc.CallOption) (*Room, error)
	JoinRoom(ctx context.Context, in *JoinRoomRequest, opts ...grpc.CallOption) (*JoinRoomResponse, error)
	LeaveRoom(ctx context.Context, in *LeaveRoomRequest, opts ...grpc.CallOption) (*emptypb.Empty, error)
	KickPlayer(ctx context.Context, in *KickPlayerRequest, opts ...grpc.CallOption) (*emptypb.Empty, error)
	SendGameAction(ctx context.Context, in *GameAction, opts ...grpc.CallOption) (*GameActionAck, error)
	EndGame(ctx context.Context, in *EndGameRequest, opts ...grpc.CallOption) (*emptypb.Empty, error)
	PersistGameState(ctx context.Context, in *PersistGameStateRequest, opts ...grpc.CallOption) (*emptypb.Empty, error)
	UpdatePlayerAttributes(ctx context.Context, in *UpdatePlayerAttributesRequest, opts ...grpc.CallOption) (*UpdatePlayerAttributesResponse, error)
	GetPlayerAttributes(ctx context.Context, in *GetPlayerAttributesRequest, opts ...grpc.CallOption) (*GetPlayerAttributesResponse, error)
	DeletePlayerAttribute(ctx context.Context, in *DeletePlayerAttributeRequest, opts ...grpc.CallOption) (*DeletePlayerAttributeResponse, error)
}

type relayServiceClient struct {
	cc grpc.ClientConnInterface
}

func NewRelayServiceClient(cc grpc.ClientConnInterface) RelayServiceClient {
	return &relayServiceClient{cc}
}

func (c *relayServiceClient) AuthenticateUser(ctx context.Context, in *AuthenticateRequest, opts ...grpc.CallOption) (*AuthenticateResponse, error) {
	out := new(AuthenticateResponse)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/AuthenticateUser", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) UpdatePlayer(ctx context.Context, in *UpdatePlayerRequest, opts ...grpc.CallOption) (*UpdatePlayerResponse, error) {
	out := new(UpdatePlayerResponse)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/UpdatePlayer", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) CreateRoom(ctx context.Context, in *CreateRoomRequest, opts ...grpc.CallOption) (*Room, error) {
	out := new(Room)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/CreateRoom", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) GetRoom(ctx context.Context, in *GetRoomRequest, opts ...grpc.CallOption) (*Room, error) {
	out := new(Room)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/GetRoom", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) DeleteRoom(ctx context.Context, in *DeleteRoomRequest, opts ...grpc.CallOption) (*emptypb.Empty, error) {
	out := new(emptypb.Empty)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/DeleteRoom", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) UpdateRoomState(ctx context.Context, in *UpdateRoomStateRequest, opts ...grpc.CallOption) (*Room, error) {
	out := new(Room)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/UpdateRoomState", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) JoinRoom(ctx context.Context, in *JoinRoomRequest, opts ...grpc.CallOption) (*JoinRoomResponse, error) {
	out := new(JoinRoomResponse)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/JoinRoom", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) LeaveRoom(ctx context.Context, in *LeaveRoomRequest, opts ...grpc.CallOption) (*emptypb.Empty, error) {
	out := new(emptypb.Empty)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/LeaveRoom", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) KickPlayer(ctx context.Context, in *KickPlayerRequest, opts ...grpc.CallOption) (*emptypb.Empty, error) {
	out := new(emptypb.Empty)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/KickPlayer", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) SendGameAction(ctx context.Context, in *GameAction, opts ...grpc.CallOption) (*GameActionAck, error) {
	out := new(GameActionAck)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/SendGameAction", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) EndGame(ctx context.Context, in *EndGameRequest, opts ...grpc.CallOption) (*emptypb.Empty, error) {
	out := new(emptypb.Empty)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/EndGame", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) PersistGameState(ctx context.Context, in *PersistGameStateRequest, opts ...grpc.CallOption) (*emptypb.Empty, error) {
	out := new(emptypb.Empty)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/PersistGameState", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) UpdatePlayerAttributes(ctx context.Context, in *UpdatePlayerAttributesRequest, opts ...grpc.CallOption) (*UpdatePlayerAttributesResponse, error) {
	out := new(UpdatePlayerAttributesResponse)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/UpdatePlayerAttributes", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) GetPlayerAttributes(ctx context.Context, in *GetPlayerAttributesRequest, opts ...grpc.CallOption) (*GetPlayerAttributesResponse, error) {
	out := new(GetPlayerAttributesResponse)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/GetPlayerAttributes", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *relayServiceClient) DeletePlayerAttribute(ctx context.Context, in *DeletePlayerAttributeRequest, opts ...grpc.CallOption) (*DeletePlayerAttributeResponse, error) {
	out := new(DeletePlayerAttributeResponse)
	err := c.cc.Invoke(ctx, "/gamecloud.RelayService/DeletePlayerAttribute", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

// RelayServiceServer is the server API for RelayService service.
// All implementations must embed UnimplementedRelayServiceServer
// for forward compatibility
type RelayServiceServer interface {
	AuthenticateUser(context.Context, *AuthenticateRequest) (*AuthenticateResponse, error)
	UpdatePlayer(context.Context, *UpdatePlayerRequest) (*UpdatePlayerResponse, error)
	CreateRoom(context.Context, *CreateRoomRequest) (*Room, error)
	GetRoom(context.Context, *GetRoomRequest) (*Room, error)
	DeleteRoom(context.Context, *DeleteRoomRequest) (*emptypb.Empty, error)
	UpdateRoomState(context.Context, *UpdateRoomStateRequest) (*Room, error)
	JoinRoom(context.Context, *JoinRoomRequest) (*JoinRoomResponse, error)
	LeaveRoom(context.Context, *LeaveRoomRequest) (*emptypb.Empty, error)
	KickPlayer(context.Context, *KickPlayerRequest) (*emptypb.Empty, error)
	SendGameAction(context.Context, *GameAction) (*GameActionAck, error)
	EndGame(context.Context, *EndGameRequest) (*emptypb.Empty, error)
	PersistGameState(context.Context, *PersistGameStateRequest) (*emptypb.Empty, error)
	UpdatePlayerAttributes(context.Context, *UpdatePlayerAttributesRequest) (*UpdatePlayerAttributesResponse, error)
	GetPlayerAttributes(context.Context, *GetPlayerAttributesRequest) (*GetPlayerAttributesResponse, error)
	DeletePlayerAttribute(context.Context, *DeletePlayerAttributeRequest) (*DeletePlayerAttributeResponse, error)
	mustEmbedUnimplementedRelayServiceServer()
}

// UnimplementedRelayServiceServer must be embedded to have forward compatible implementations.
type UnimplementedRelayServiceServer struct {
}

func (UnimplementedRelayServiceServer) AuthenticateUser(context.Context, *AuthenticateRequest) (*AuthenticateResponse, error) {
	return nil, status.Errorf(codes.Unimplemented, "method AuthenticateUser not implemented")
}
func (UnimplementedRelayServiceServer) UpdatePlayer(context.Context, *UpdatePlayerRequest) (*UpdatePlayerResponse, error) {
	return nil, status.Errorf(codes.Unimplemented, "method UpdatePlayer not implemented")
}
func (UnimplementedRelayServiceServer) CreateRoom(context.Context, *CreateRoomRequest) (*Room, error) {
	return nil, status.Errorf(codes.Unimplemented, "method CreateRoom not implemented")
}
func (UnimplementedRelayServiceServer) GetRoom(context.Context, *GetRoomRequest) (*Room, error) {
	return nil, status.Errorf(codes.Unimplemented, "method GetRoom not implemented")
}
func (UnimplementedRelayServiceServer) DeleteRoom(context.Context, *DeleteRoomRequest) (*emptypb.Empty, error) {
	return nil, status.Errorf(codes.Unimplemented, "method DeleteRoom not implemented")
}
func (UnimplementedRelayServiceServer) UpdateRoomState(context.Context, *UpdateRoomStateRequest) (*Room, error) {
	return nil, status.Errorf(codes.Unimplemented, "method UpdateRoomState not implemented")
}
func (UnimplementedRelayServiceServer) JoinRoom(context.Context, *JoinRoomRequest) (*JoinRoomResponse, error) {
	return nil, status.Errorf(codes.Unimplemented, "method JoinRoom not implemented")
}
func (UnimplementedRelayServiceServer) LeaveRoom(context.Context, *LeaveRoomRequest) (*emptypb.Empty, error) {
	return nil, status.Errorf(codes.Unimplemented, "method LeaveRoom not implemented")
}
func (UnimplementedRelayServiceServer) KickPlayer(context.Context, *KickPlayerRequest) (*emptypb.Empty, error) {
	return nil, status.Errorf(codes.Unimplemented, "method KickPlayer not implemented")
}
func (UnimplementedRelayServiceServer) SendGameAction(context.Context, *GameAction) (*GameActionAck, error) {
	return nil, status.Errorf(codes.Unimplemented, "method SendGameAction not implemented")
}
func (UnimplementedRelayServiceServer) EndGame(context.Context, *EndGameRequest) (*emptypb.Empty, error) {
	return nil, status.Errorf(codes.Unimplemented, "method EndGame not implemented")
}
func (UnimplementedRelayServiceServer) PersistGameState(context.Context, *PersistGameStateRequest) (*emptypb.Empty, error) {
	return nil, status.Errorf(codes.Unimplemented, "method PersistGameState not implemented")
}
func (UnimplementedRelayServiceServer) UpdatePlayerAttributes(context.Context, *UpdatePlayerAttributesRequest) (*UpdatePlayerAttributesResponse, error) {
	return nil, status.Errorf(codes.Unimplemented, "method UpdatePlayerAttributes not implemented")
}
func (UnimplementedRelayServiceServer) GetPlayerAttributes(context.Context, *GetPlayerAttributesRequest) (*GetPlayerAttributesResponse, error) {
	return nil, status.Errorf(codes.Unimplemented, "method GetPlayerAttributes not implemented")
}
func (UnimplementedRelayServiceServer) DeletePlayerAttribute(context.Context, *DeletePlayerAttributeRequest) (*DeletePlayerAttributeResponse, error) {
	return nil, status.Errorf(codes.Unimplemented, "method DeletePlayerAttribute not implemented")
}
func (UnimplementedRelayServiceServer) mustEmbedUnimplementedRelayServiceServer() {}

// UnsafeRelayServiceServer may be embedded to opt out of forward compatibility for this service.
// Use of this interface is not recommended, as added methods to RelayServiceServer will
// result in compilation errors.
type UnsafeRelayServiceServer interface {
	mustEmbedUnimplementedRelayServiceServer()
}

func RegisterRelayServiceServer(s grpc.ServiceRegistrar, srv RelayServiceServer) {
	s.RegisterService(&RelayService_ServiceDesc, srv)
}

func _RelayService_AuthenticateUser_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(AuthenticateRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).AuthenticateUser(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/AuthenticateUser",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).AuthenticateUser(ctx, req.(*AuthenticateRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_UpdatePlayer_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(UpdatePlayerRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).UpdatePlayer(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/UpdatePlayer",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).UpdatePlayer(ctx, req.(*UpdatePlayerRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_CreateRoom_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(CreateRoomRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).CreateRoom(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/CreateRoom",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).CreateRoom(ctx, req.(*CreateRoomRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_GetRoom_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(GetRoomRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).GetRoom(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/GetRoom",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).GetRoom(ctx, req.(*GetRoomRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_DeleteRoom_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(DeleteRoomRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).DeleteRoom(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/DeleteRoom",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).DeleteRoom(ctx, req.(*DeleteRoomRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_UpdateRoomState_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(UpdateRoomStateRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).UpdateRoomState(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/UpdateRoomState",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).UpdateRoomState(ctx, req.(*UpdateRoomStateRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_JoinRoom_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(JoinRoomRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).JoinRoom(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/JoinRoom",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).JoinRoom(ctx, req.(*JoinRoomRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_LeaveRoom_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(LeaveRoomRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).LeaveRoom(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/LeaveRoom",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).LeaveRoom(ctx, req.(*LeaveRoomRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_KickPlayer_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(KickPlayerRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).KickPlayer(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/KickPlayer",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).KickPlayer(ctx, req.(*KickPlayerRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_SendGameAction_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(GameAction)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).SendGameAction(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/SendGameAction",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).SendGameAction(ctx, req.(*GameAction))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_EndGame_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(EndGameRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).EndGame(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/EndGame",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).EndGame(ctx, req.(*EndGameRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_PersistGameState_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(PersistGameStateRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).PersistGameState(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/PersistGameState",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).PersistGameState(ctx, req.(*PersistGameStateRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_UpdatePlayerAttributes_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(UpdatePlayerAttributesRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).UpdatePlayerAttributes(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/UpdatePlayerAttributes",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).UpdatePlayerAttributes(ctx, req.(*UpdatePlayerAttributesRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_GetPlayerAttributes_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(GetPlayerAttributesRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).GetPlayerAttributes(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/GetPlayerAttributes",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).GetPlayerAttributes(ctx, req.(*GetPlayerAttributesRequest))
	}
	return interceptor(ctx, in, info, handler)
}

func _RelayService_DeletePlayerAttribute_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(DeletePlayerAttributeRequest)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(RelayServiceServer).DeletePlayerAttribute(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.RelayService/DeletePlayerAttribute",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(RelayServiceServer).DeletePlayerAttribute(ctx, req.(*DeletePlayerAttributeRequest))
	}
	return interceptor(ctx, in, info, handler)
}

// RelayService_ServiceDesc is the grpc.ServiceDesc for RelayService service.
// It's only intended for direct use with grpc.RegisterService,
// and not to be introspected or modified (even as a copy)
var RelayService_ServiceDesc = grpc.ServiceDesc{
	ServiceName: "gamecloud.RelayService",
	HandlerType: (*RelayServiceServer)(nil),
	Methods: []grpc.MethodDesc{
		{
			MethodName: "AuthenticateUser",
			Handler:    _RelayService_AuthenticateUser_Handler,
		},
		{
			MethodName: "UpdatePlayer",
			Handler:    _RelayService_UpdatePlayer_Handler,
		},
		{
			MethodName: "CreateRoom",
			Handler:    _RelayService_CreateRoom_Handler,
		},
		{
			MethodName: "GetRoom",
			Handler:    _RelayService_GetRoom_Handler,
		},
		{
			MethodName: "DeleteRoom",
			Handler:    _RelayService_DeleteRoom_Handler,
		},
		{
			MethodName: "UpdateRoomState",
			Handler:    _RelayService_UpdateRoomState_Handler,
		},
		{
			MethodName: "JoinRoom",
			Handler:    _RelayService_JoinRoom_Handler,
		},
		{
			MethodName: "LeaveRoom",
			Handler:    _RelayService_LeaveRoom_Handler,
		},
		{
			MethodName: "KickPlayer",
			Handler:    _RelayService_KickPlayer_Handler,
		},
		{
			MethodName: "SendGameAction",
			Handler:    _RelayService_SendGameAction_Handler,
		},
		{
			MethodName: "EndGame",
			Handler:    _RelayService_EndGame_Handler,
		},
		{
			MethodName: "PersistGameState",
			Handler:    _RelayService_PersistGameState_Handler,
		},
		{
			MethodName: "UpdatePlayerAttributes",
			Handler:    _RelayService_UpdatePlayerAttributes_Handler,
		},
		{
			MethodName: "GetPlayerAttributes",
			Handler:    _RelayService_GetPlayerAttributes_Handler,
		},
		{
			MethodName: "DeletePlayerAttribute",
			Handler:    _RelayService_DeletePlayerAttribute_Handler,
		},
	},
	Streams:  []grpc.StreamDesc{},
	Metadata: "gamecloud.proto",
}

// GameEventServiceClient is the client API for GameEventService service.
//
// For semantics around ctx use and closing/ending streaming RPCs, please refer to https://pkg.go.dev/google.golang.org/grpc/?tab=doc#ClientConn.NewStream.
type GameEventServiceClient interface {
	OnPlayerJoined(ctx context.Context, in *PlayerEvent, opts ...grpc.CallOption) (*EventAck, error)
	OnPlayerLeft(ctx context.Context, in *PlayerEvent, opts ...grpc.CallOption) (*EventAck, error)
	OnRoomClosed(ctx context.Context, in *RoomEvent, opts ...grpc.CallOption) (*EventAck, error)
	OnGameCompleted(ctx context.Context, in *GameCompletedEvent, opts ...grpc.CallOption) (*EventAck, error)
	OnGameStateChanged(ctx context.Context, in *GameStateEvent, opts ...grpc.CallOption) (*EventAck, error)
	OnMatchmakingResult(ctx context.Context, in *MatchmakingResultEvent, opts ...grpc.CallOption) (*EventAck, error)
}

type gameEventServiceClient struct {
	cc grpc.ClientConnInterface
}

func NewGameEventServiceClient(cc grpc.ClientConnInterface) GameEventServiceClient {
	return &gameEventServiceClient{cc}
}

func (c *gameEventServiceClient) OnPlayerJoined(ctx context.Context, in *PlayerEvent, opts ...grpc.CallOption) (*EventAck, error) {
	out := new(EventAck)
	err := c.cc.Invoke(ctx, "/gamecloud.GameEventService/OnPlayerJoined", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *gameEventServiceClient) OnPlayerLeft(ctx context.Context, in *PlayerEvent, opts ...grpc.CallOption) (*EventAck, error) {
	out := new(EventAck)
	err := c.cc.Invoke(ctx, "/gamecloud.GameEventService/OnPlayerLeft", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *gameEventServiceClient) OnRoomClosed(ctx context.Context, in *RoomEvent, opts ...grpc.CallOption) (*EventAck, error) {
	out := new(EventAck)
	err := c.cc.Invoke(ctx, "/gamecloud.GameEventService/OnRoomClosed", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *gameEventServiceClient) OnGameCompleted(ctx context.Context, in *GameCompletedEvent, opts ...grpc.CallOption) (*EventAck, error) {
	out := new(EventAck)
	err := c.cc.Invoke(ctx, "/gamecloud.GameEventService/OnGameCompleted", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *gameEventServiceClient) OnGameStateChanged(ctx context.Context, in *GameStateEvent, opts ...grpc.CallOption) (*EventAck, error) {
	out := new(EventAck)
	err := c.cc.Invoke(ctx, "/gamecloud.GameEventService/OnGameStateChanged", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

func (c *gameEventServiceClient) OnMatchmakingResult(ctx context.Context, in *MatchmakingResultEvent, opts ...grpc.CallOption) (*EventAck, error) {
	out := new(EventAck)
	err := c.cc.Invoke(ctx, "/gamecloud.GameEventService/OnMatchmakingResult", in, out, opts...)
	if err != nil {
		return nil, err
	}
	return out, nil
}

// GameEventServiceServer is the server API for GameEventService service.
// All implementations must embed UnimplementedGameEventServiceServer
// for forward compatibility
type GameEventServiceServer interface {
	OnPlayerJoined(context.Context, *PlayerEvent) (*EventAck, error)
	OnPlayerLeft(context.Context, *PlayerEvent) (*EventAck, error)
	OnRoomClosed(context.Context, *RoomEvent) (*EventAck, error)
	OnGameCompleted(context.Context, *GameCompletedEvent) (*EventAck, error)
	OnGameStateChanged(context.Context, *GameStateEvent) (*EventAck, error)
	OnMatchmakingResult(context.Context, *MatchmakingResultEvent) (*EventAck, error)
	mustEmbedUnimplementedGameEventServiceServer()
}

// UnimplementedGameEventServiceServer must be embedded to have forward compatible implementations.
type UnimplementedGameEventServiceServer struct {
}

func (UnimplementedGameEventServiceServer) OnPlayerJoined(context.Context, *PlayerEvent) (*EventAck, error) {
	return nil, status.Errorf(codes.Unimplemented, "method OnPlayerJoined not implemented")
}
func (UnimplementedGameEventServiceServer) OnPlayerLeft(context.Context, *PlayerEvent) (*EventAck, error) {
	return nil, status.Errorf(codes.Unimplemented, "method OnPlayerLeft not implemented")
}
func (UnimplementedGameEventServiceServer) OnRoomClosed(context.Context, *RoomEvent) (*EventAck, error) {
	return nil, status.Errorf(codes.Unimplemented, "method OnRoomClosed not implemented")
}
func (UnimplementedGameEventServiceServer) OnGameCompleted(context.Context, *GameCompletedEvent) (*EventAck, error) {
	return nil, status.Errorf(codes.Unimplemented, "method OnGameCompleted not implemented")
}
func (UnimplementedGameEventServiceServer) OnGameStateChanged(context.Context, *GameStateEvent) (*EventAck, error) {
	return nil, status.Errorf(codes.Unimplemented, "method OnGameStateChanged not implemented")
}
func (UnimplementedGameEventServiceServer) OnMatchmakingResult(context.Context, *MatchmakingResultEvent) (*EventAck, error) {
	return nil, status.Errorf(codes.Unimplemented, "method OnMatchmakingResult not implemented")
}
func (UnimplementedGameEventServiceServer) mustEmbedUnimplementedGameEventServiceServer() {}

// UnsafeGameEventServiceServer may be embedded to opt out of forward compatibility for this service.
// Use of this interface is not recommended, as added methods to GameEventServiceServer will
// result in compilation errors.
type UnsafeGameEventServiceServer interface {
	mustEmbedUnimplementedGameEventServiceServer()
}

func RegisterGameEventServiceServer(s grpc.ServiceRegistrar, srv GameEventServiceServer) {
	s.RegisterService(&GameEventService_ServiceDesc, srv)
}

func _GameEventService_OnPlayerJoined_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(PlayerEvent)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(GameEventServiceServer).OnPlayerJoined(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.GameEventService/OnPlayerJoined",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(GameEventServiceServer).OnPlayerJoined(ctx, req.(*PlayerEvent))
	}
	return interceptor(ctx, in, info, handler)
}

func _GameEventService_OnPlayerLeft_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(PlayerEvent)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(GameEventServiceServer).OnPlayerLeft(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.GameEventService/OnPlayerLeft",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(GameEventServiceServer).OnPlayerLeft(ctx, req.(*PlayerEvent))
	}
	return interceptor(ctx, in, info, handler)
}

func _GameEventService_OnRoomClosed_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(RoomEvent)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(GameEventServiceServer).OnRoomClosed(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.GameEventService/OnRoomClosed",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(GameEventServiceServer).OnRoomClosed(ctx, req.(*RoomEvent))
	}
	return interceptor(ctx, in, info, handler)
}

func _GameEventService_OnGameCompleted_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(GameCompletedEvent)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(GameEventServiceServer).OnGameCompleted(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.GameEventService/OnGameCompleted",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(GameEventServiceServer).OnGameCompleted(ctx, req.(*GameCompletedEvent))
	}
	return interceptor(ctx, in, info, handler)
}

func _GameEventService_OnGameStateChanged_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(GameStateEvent)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(GameEventServiceServer).OnGameStateChanged(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.GameEventService/OnGameStateChanged",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(GameEventServiceServer).OnGameStateChanged(ctx, req.(*GameStateEvent))
	}
	return interceptor(ctx, in, info, handler)
}

func _GameEventService_OnMatchmakingResult_Handler(srv interface{}, ctx context.Context, dec func(interface{}) error, interceptor grpc.UnaryServerInterceptor) (interface{}, error) {
	in := new(MatchmakingResultEvent)
	if err := dec(in); err != nil {
		return nil, err
	}
	if interceptor == nil {
		return srv.(GameEventServiceServer).OnMatchmakingResult(ctx, in)
	}
	info := &grpc.UnaryServerInfo{
		Server:     srv,
		FullMethod: "/gamecloud.GameEventService/OnMatchmakingResult",
	}
	handler := func(ctx context.Context, req interface{}) (interface{}, error) {
		return srv.(GameEventServiceServer).OnMatchmakingResult(ctx, req.(*MatchmakingResultEvent))
	}
	return interceptor(ctx, in, info, handler)
}

// GameEventService_ServiceDesc is the grpc.ServiceDesc for GameEventService service.
// It's only intended for direct use with grpc.RegisterService,
// and not to be introspected or modified (even as a copy)
var GameEventService_ServiceDesc = grpc.ServiceDesc{
	ServiceName: "gamecloud.GameEventService",
	HandlerType: (*GameEventServiceServer)(nil),
	Methods: []grpc.MethodDesc{
		{
			MethodName: "OnPlayerJoined",
			Handler:    _GameEventService_OnPlayerJoined_Handler,
		},
		{
			MethodName: "OnPlayerLeft",
			Handler:    _GameEventService_OnPlayerLeft_Handler,
		},
		{
			MethodName: "OnRoomClosed",
			Handler:    _GameEventService_OnRoomClosed_Handler,
		},
		{
			MethodName: "OnGameCompleted",
			Handler:    _GameEventService_OnGameCompleted_Handler,
		},
		{
			MethodName: "OnGameStateChanged",
			Handler:    _GameEventService_OnGameStateChanged_Handler,
		},
		{
			MethodName: "OnMatchmakingResult",
			Handler:    _GameEventService_OnMatchmakingResult_Handler,
		},
	},
	Streams:  []grpc.StreamDesc{},
	Metadata: "gamecloud.proto",
}
