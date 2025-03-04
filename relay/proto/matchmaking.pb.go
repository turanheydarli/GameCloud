// Code generated by protoc-gen-go. DO NOT EDIT.
// versions:
// 	protoc-gen-go v1.28.1
// 	protoc        v5.29.3
// source: matchmaking.proto

package proto

import (
	protoreflect "google.golang.org/protobuf/reflect/protoreflect"
	protoimpl "google.golang.org/protobuf/runtime/protoimpl"
	emptypb "google.golang.org/protobuf/types/known/emptypb"
	timestamppb "google.golang.org/protobuf/types/known/timestamppb"
	reflect "reflect"
	sync "sync"
)

const (
	// Verify that this generated code is sufficiently up-to-date.
	_ = protoimpl.EnforceVersion(20 - protoimpl.MinVersion)
	// Verify that runtime/protoimpl is sufficiently up-to-date.
	_ = protoimpl.EnforceVersion(protoimpl.MaxVersion - 20)
)

type MatchStatus int32

const (
	MatchStatus_MATCH_STATUS_UNSPECIFIED MatchStatus = 0
	MatchStatus_MATCH_STATUS_QUEUED      MatchStatus = 1
	MatchStatus_MATCH_STATUS_MATCHED     MatchStatus = 2
	MatchStatus_MATCH_STATUS_CANCELLED   MatchStatus = 3
	MatchStatus_MATCH_STATUS_FAILED      MatchStatus = 4
	MatchStatus_MATCH_STATUS_TIMED_OUT   MatchStatus = 5
)

// Enum value maps for MatchStatus.
var (
	MatchStatus_name = map[int32]string{
		0: "MATCH_STATUS_UNSPECIFIED",
		1: "MATCH_STATUS_QUEUED",
		2: "MATCH_STATUS_MATCHED",
		3: "MATCH_STATUS_CANCELLED",
		4: "MATCH_STATUS_FAILED",
		5: "MATCH_STATUS_TIMED_OUT",
	}
	MatchStatus_value = map[string]int32{
		"MATCH_STATUS_UNSPECIFIED": 0,
		"MATCH_STATUS_QUEUED":      1,
		"MATCH_STATUS_MATCHED":     2,
		"MATCH_STATUS_CANCELLED":   3,
		"MATCH_STATUS_FAILED":      4,
		"MATCH_STATUS_TIMED_OUT":   5,
	}
)

func (x MatchStatus) Enum() *MatchStatus {
	p := new(MatchStatus)
	*p = x
	return p
}

func (x MatchStatus) String() string {
	return protoimpl.X.EnumStringOf(x.Descriptor(), protoreflect.EnumNumber(x))
}

func (MatchStatus) Descriptor() protoreflect.EnumDescriptor {
	return file_matchmaking_proto_enumTypes[0].Descriptor()
}

func (MatchStatus) Type() protoreflect.EnumType {
	return &file_matchmaking_proto_enumTypes[0]
}

func (x MatchStatus) Number() protoreflect.EnumNumber {
	return protoreflect.EnumNumber(x)
}

// Deprecated: Use MatchStatus.Descriptor instead.
func (MatchStatus) EnumDescriptor() ([]byte, []int) {
	return file_matchmaking_proto_rawDescGZIP(), []int{0}
}

type CreateTicketRequest struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	PalyerId   string               `protobuf:"bytes,1,opt,name=palyer_id,json=palyerId,proto3" json:"palyer_id,omitempty"`
	GameId     string               `protobuf:"bytes,2,opt,name=game_id,json=gameId,proto3" json:"game_id,omitempty"`
	QueueName  string               `protobuf:"bytes,3,opt,name=queue_name,json=queueName,proto3" json:"queue_name,omitempty"`
	Criteria   *MatchmakingCriteria `protobuf:"bytes,4,opt,name=criteria,proto3" json:"criteria,omitempty"`
	Properties map[string]string    `protobuf:"bytes,5,rep,name=properties,proto3" json:"properties,omitempty" protobuf_key:"bytes,1,opt,name=key,proto3" protobuf_val:"bytes,2,opt,name=value,proto3"`
	SessionId  string               `protobuf:"bytes,6,opt,name=session_id,json=sessionId,proto3" json:"session_id,omitempty"`
}

func (x *CreateTicketRequest) Reset() {
	*x = CreateTicketRequest{}
	if protoimpl.UnsafeEnabled {
		mi := &file_matchmaking_proto_msgTypes[0]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *CreateTicketRequest) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*CreateTicketRequest) ProtoMessage() {}

func (x *CreateTicketRequest) ProtoReflect() protoreflect.Message {
	mi := &file_matchmaking_proto_msgTypes[0]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use CreateTicketRequest.ProtoReflect.Descriptor instead.
func (*CreateTicketRequest) Descriptor() ([]byte, []int) {
	return file_matchmaking_proto_rawDescGZIP(), []int{0}
}

func (x *CreateTicketRequest) GetPalyerId() string {
	if x != nil {
		return x.PalyerId
	}
	return ""
}

func (x *CreateTicketRequest) GetGameId() string {
	if x != nil {
		return x.GameId
	}
	return ""
}

func (x *CreateTicketRequest) GetQueueName() string {
	if x != nil {
		return x.QueueName
	}
	return ""
}

func (x *CreateTicketRequest) GetCriteria() *MatchmakingCriteria {
	if x != nil {
		return x.Criteria
	}
	return nil
}

func (x *CreateTicketRequest) GetProperties() map[string]string {
	if x != nil {
		return x.Properties
	}
	return nil
}

func (x *CreateTicketRequest) GetSessionId() string {
	if x != nil {
		return x.SessionId
	}
	return ""
}

type MatchmakingCriteria struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	MinPlayers         int32             `protobuf:"varint,1,opt,name=min_players,json=minPlayers,proto3" json:"min_players,omitempty"`
	MaxPlayers         int32             `protobuf:"varint,2,opt,name=max_players,json=maxPlayers,proto3" json:"max_players,omitempty"`
	Pool               string            `protobuf:"bytes,3,opt,name=pool,proto3" json:"pool,omitempty"`
	SkillLevel         string            `protobuf:"bytes,4,opt,name=skill_level,json=skillLevel,proto3" json:"skill_level,omitempty"`
	RequiredProperties []string          `protobuf:"bytes,5,rep,name=required_properties,json=requiredProperties,proto3" json:"required_properties,omitempty"`
	Filters            map[string]string `protobuf:"bytes,6,rep,name=filters,proto3" json:"filters,omitempty" protobuf_key:"bytes,1,opt,name=key,proto3" protobuf_val:"bytes,2,opt,name=value,proto3"`
}

func (x *MatchmakingCriteria) Reset() {
	*x = MatchmakingCriteria{}
	if protoimpl.UnsafeEnabled {
		mi := &file_matchmaking_proto_msgTypes[1]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *MatchmakingCriteria) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*MatchmakingCriteria) ProtoMessage() {}

func (x *MatchmakingCriteria) ProtoReflect() protoreflect.Message {
	mi := &file_matchmaking_proto_msgTypes[1]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use MatchmakingCriteria.ProtoReflect.Descriptor instead.
func (*MatchmakingCriteria) Descriptor() ([]byte, []int) {
	return file_matchmaking_proto_rawDescGZIP(), []int{1}
}

func (x *MatchmakingCriteria) GetMinPlayers() int32 {
	if x != nil {
		return x.MinPlayers
	}
	return 0
}

func (x *MatchmakingCriteria) GetMaxPlayers() int32 {
	if x != nil {
		return x.MaxPlayers
	}
	return 0
}

func (x *MatchmakingCriteria) GetPool() string {
	if x != nil {
		return x.Pool
	}
	return ""
}

func (x *MatchmakingCriteria) GetSkillLevel() string {
	if x != nil {
		return x.SkillLevel
	}
	return ""
}

func (x *MatchmakingCriteria) GetRequiredProperties() []string {
	if x != nil {
		return x.RequiredProperties
	}
	return nil
}

func (x *MatchmakingCriteria) GetFilters() map[string]string {
	if x != nil {
		return x.Filters
	}
	return nil
}

type MatchmakingTicket struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	TicketId   string                 `protobuf:"bytes,1,opt,name=ticket_id,json=ticketId,proto3" json:"ticket_id,omitempty"`
	UserId     string                 `protobuf:"bytes,2,opt,name=user_id,json=userId,proto3" json:"user_id,omitempty"`
	GameId     string                 `protobuf:"bytes,3,opt,name=game_id,json=gameId,proto3" json:"game_id,omitempty"`
	Criteria   *MatchmakingCriteria   `protobuf:"bytes,4,opt,name=criteria,proto3" json:"criteria,omitempty"`
	Properties map[string]string      `protobuf:"bytes,5,rep,name=properties,proto3" json:"properties,omitempty" protobuf_key:"bytes,1,opt,name=key,proto3" protobuf_val:"bytes,2,opt,name=value,proto3"`
	Status     MatchStatus            `protobuf:"varint,6,opt,name=status,proto3,enum=gamecloud.MatchStatus" json:"status,omitempty"`
	RoomId     string                 `protobuf:"bytes,7,opt,name=room_id,json=roomId,proto3" json:"room_id,omitempty"`
	CreatedAt  *timestamppb.Timestamp `protobuf:"bytes,8,opt,name=created_at,json=createdAt,proto3" json:"created_at,omitempty"`
	UpdatedAt  *timestamppb.Timestamp `protobuf:"bytes,9,opt,name=updated_at,json=updatedAt,proto3" json:"updated_at,omitempty"`
}

func (x *MatchmakingTicket) Reset() {
	*x = MatchmakingTicket{}
	if protoimpl.UnsafeEnabled {
		mi := &file_matchmaking_proto_msgTypes[2]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *MatchmakingTicket) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*MatchmakingTicket) ProtoMessage() {}

func (x *MatchmakingTicket) ProtoReflect() protoreflect.Message {
	mi := &file_matchmaking_proto_msgTypes[2]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use MatchmakingTicket.ProtoReflect.Descriptor instead.
func (*MatchmakingTicket) Descriptor() ([]byte, []int) {
	return file_matchmaking_proto_rawDescGZIP(), []int{2}
}

func (x *MatchmakingTicket) GetTicketId() string {
	if x != nil {
		return x.TicketId
	}
	return ""
}

func (x *MatchmakingTicket) GetUserId() string {
	if x != nil {
		return x.UserId
	}
	return ""
}

func (x *MatchmakingTicket) GetGameId() string {
	if x != nil {
		return x.GameId
	}
	return ""
}

func (x *MatchmakingTicket) GetCriteria() *MatchmakingCriteria {
	if x != nil {
		return x.Criteria
	}
	return nil
}

func (x *MatchmakingTicket) GetProperties() map[string]string {
	if x != nil {
		return x.Properties
	}
	return nil
}

func (x *MatchmakingTicket) GetStatus() MatchStatus {
	if x != nil {
		return x.Status
	}
	return MatchStatus_MATCH_STATUS_UNSPECIFIED
}

func (x *MatchmakingTicket) GetRoomId() string {
	if x != nil {
		return x.RoomId
	}
	return ""
}

func (x *MatchmakingTicket) GetCreatedAt() *timestamppb.Timestamp {
	if x != nil {
		return x.CreatedAt
	}
	return nil
}

func (x *MatchmakingTicket) GetUpdatedAt() *timestamppb.Timestamp {
	if x != nil {
		return x.UpdatedAt
	}
	return nil
}

type DeleteTicketRequest struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	TicketId string `protobuf:"bytes,1,opt,name=ticket_id,json=ticketId,proto3" json:"ticket_id,omitempty"`
}

func (x *DeleteTicketRequest) Reset() {
	*x = DeleteTicketRequest{}
	if protoimpl.UnsafeEnabled {
		mi := &file_matchmaking_proto_msgTypes[3]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *DeleteTicketRequest) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*DeleteTicketRequest) ProtoMessage() {}

func (x *DeleteTicketRequest) ProtoReflect() protoreflect.Message {
	mi := &file_matchmaking_proto_msgTypes[3]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use DeleteTicketRequest.ProtoReflect.Descriptor instead.
func (*DeleteTicketRequest) Descriptor() ([]byte, []int) {
	return file_matchmaking_proto_rawDescGZIP(), []int{3}
}

func (x *DeleteTicketRequest) GetTicketId() string {
	if x != nil {
		return x.TicketId
	}
	return ""
}

type GetTicketRequest struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	TicketId string `protobuf:"bytes,1,opt,name=ticket_id,json=ticketId,proto3" json:"ticket_id,omitempty"`
}

func (x *GetTicketRequest) Reset() {
	*x = GetTicketRequest{}
	if protoimpl.UnsafeEnabled {
		mi := &file_matchmaking_proto_msgTypes[4]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *GetTicketRequest) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*GetTicketRequest) ProtoMessage() {}

func (x *GetTicketRequest) ProtoReflect() protoreflect.Message {
	mi := &file_matchmaking_proto_msgTypes[4]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use GetTicketRequest.ProtoReflect.Descriptor instead.
func (*GetTicketRequest) Descriptor() ([]byte, []int) {
	return file_matchmaking_proto_rawDescGZIP(), []int{4}
}

func (x *GetTicketRequest) GetTicketId() string {
	if x != nil {
		return x.TicketId
	}
	return ""
}

type AddUsersToMatchRequest struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	UserIds  []string             `protobuf:"bytes,1,rep,name=user_ids,json=userIds,proto3" json:"user_ids,omitempty"`
	GameId   string               `protobuf:"bytes,2,opt,name=game_id,json=gameId,proto3" json:"game_id,omitempty"`
	Criteria *MatchmakingCriteria `protobuf:"bytes,3,opt,name=criteria,proto3" json:"criteria,omitempty"`
}

func (x *AddUsersToMatchRequest) Reset() {
	*x = AddUsersToMatchRequest{}
	if protoimpl.UnsafeEnabled {
		mi := &file_matchmaking_proto_msgTypes[5]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *AddUsersToMatchRequest) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*AddUsersToMatchRequest) ProtoMessage() {}

func (x *AddUsersToMatchRequest) ProtoReflect() protoreflect.Message {
	mi := &file_matchmaking_proto_msgTypes[5]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use AddUsersToMatchRequest.ProtoReflect.Descriptor instead.
func (*AddUsersToMatchRequest) Descriptor() ([]byte, []int) {
	return file_matchmaking_proto_rawDescGZIP(), []int{5}
}

func (x *AddUsersToMatchRequest) GetUserIds() []string {
	if x != nil {
		return x.UserIds
	}
	return nil
}

func (x *AddUsersToMatchRequest) GetGameId() string {
	if x != nil {
		return x.GameId
	}
	return ""
}

func (x *AddUsersToMatchRequest) GetCriteria() *MatchmakingCriteria {
	if x != nil {
		return x.Criteria
	}
	return nil
}

var File_matchmaking_proto protoreflect.FileDescriptor

var file_matchmaking_proto_rawDesc = []byte{
	0x0a, 0x11, 0x6d, 0x61, 0x74, 0x63, 0x68, 0x6d, 0x61, 0x6b, 0x69, 0x6e, 0x67, 0x2e, 0x70, 0x72,
	0x6f, 0x74, 0x6f, 0x12, 0x09, 0x67, 0x61, 0x6d, 0x65, 0x63, 0x6c, 0x6f, 0x75, 0x64, 0x1a, 0x1b,
	0x67, 0x6f, 0x6f, 0x67, 0x6c, 0x65, 0x2f, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x62, 0x75, 0x66, 0x2f,
	0x65, 0x6d, 0x70, 0x74, 0x79, 0x2e, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x1a, 0x1f, 0x67, 0x6f, 0x6f,
	0x67, 0x6c, 0x65, 0x2f, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x62, 0x75, 0x66, 0x2f, 0x74, 0x69, 0x6d,
	0x65, 0x73, 0x74, 0x61, 0x6d, 0x70, 0x2e, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x22, 0xd4, 0x02, 0x0a,
	0x13, 0x43, 0x72, 0x65, 0x61, 0x74, 0x65, 0x54, 0x69, 0x63, 0x6b, 0x65, 0x74, 0x52, 0x65, 0x71,
	0x75, 0x65, 0x73, 0x74, 0x12, 0x1b, 0x0a, 0x09, 0x70, 0x61, 0x6c, 0x79, 0x65, 0x72, 0x5f, 0x69,
	0x64, 0x18, 0x01, 0x20, 0x01, 0x28, 0x09, 0x52, 0x08, 0x70, 0x61, 0x6c, 0x79, 0x65, 0x72, 0x49,
	0x64, 0x12, 0x17, 0x0a, 0x07, 0x67, 0x61, 0x6d, 0x65, 0x5f, 0x69, 0x64, 0x18, 0x02, 0x20, 0x01,
	0x28, 0x09, 0x52, 0x06, 0x67, 0x61, 0x6d, 0x65, 0x49, 0x64, 0x12, 0x1d, 0x0a, 0x0a, 0x71, 0x75,
	0x65, 0x75, 0x65, 0x5f, 0x6e, 0x61, 0x6d, 0x65, 0x18, 0x03, 0x20, 0x01, 0x28, 0x09, 0x52, 0x09,
	0x71, 0x75, 0x65, 0x75, 0x65, 0x4e, 0x61, 0x6d, 0x65, 0x12, 0x3a, 0x0a, 0x08, 0x63, 0x72, 0x69,
	0x74, 0x65, 0x72, 0x69, 0x61, 0x18, 0x04, 0x20, 0x01, 0x28, 0x0b, 0x32, 0x1e, 0x2e, 0x67, 0x61,
	0x6d, 0x65, 0x63, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x6d, 0x61, 0x6b,
	0x69, 0x6e, 0x67, 0x43, 0x72, 0x69, 0x74, 0x65, 0x72, 0x69, 0x61, 0x52, 0x08, 0x63, 0x72, 0x69,
	0x74, 0x65, 0x72, 0x69, 0x61, 0x12, 0x4e, 0x0a, 0x0a, 0x70, 0x72, 0x6f, 0x70, 0x65, 0x72, 0x74,
	0x69, 0x65, 0x73, 0x18, 0x05, 0x20, 0x03, 0x28, 0x0b, 0x32, 0x2e, 0x2e, 0x67, 0x61, 0x6d, 0x65,
	0x63, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x43, 0x72, 0x65, 0x61, 0x74, 0x65, 0x54, 0x69, 0x63, 0x6b,
	0x65, 0x74, 0x52, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x2e, 0x50, 0x72, 0x6f, 0x70, 0x65, 0x72,
	0x74, 0x69, 0x65, 0x73, 0x45, 0x6e, 0x74, 0x72, 0x79, 0x52, 0x0a, 0x70, 0x72, 0x6f, 0x70, 0x65,
	0x72, 0x74, 0x69, 0x65, 0x73, 0x12, 0x1d, 0x0a, 0x0a, 0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e,
	0x5f, 0x69, 0x64, 0x18, 0x06, 0x20, 0x01, 0x28, 0x09, 0x52, 0x09, 0x73, 0x65, 0x73, 0x73, 0x69,
	0x6f, 0x6e, 0x49, 0x64, 0x1a, 0x3d, 0x0a, 0x0f, 0x50, 0x72, 0x6f, 0x70, 0x65, 0x72, 0x74, 0x69,
	0x65, 0x73, 0x45, 0x6e, 0x74, 0x72, 0x79, 0x12, 0x10, 0x0a, 0x03, 0x6b, 0x65, 0x79, 0x18, 0x01,
	0x20, 0x01, 0x28, 0x09, 0x52, 0x03, 0x6b, 0x65, 0x79, 0x12, 0x14, 0x0a, 0x05, 0x76, 0x61, 0x6c,
	0x75, 0x65, 0x18, 0x02, 0x20, 0x01, 0x28, 0x09, 0x52, 0x05, 0x76, 0x61, 0x6c, 0x75, 0x65, 0x3a,
	0x02, 0x38, 0x01, 0x22, 0xc0, 0x02, 0x0a, 0x13, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x6d, 0x61, 0x6b,
	0x69, 0x6e, 0x67, 0x43, 0x72, 0x69, 0x74, 0x65, 0x72, 0x69, 0x61, 0x12, 0x1f, 0x0a, 0x0b, 0x6d,
	0x69, 0x6e, 0x5f, 0x70, 0x6c, 0x61, 0x79, 0x65, 0x72, 0x73, 0x18, 0x01, 0x20, 0x01, 0x28, 0x05,
	0x52, 0x0a, 0x6d, 0x69, 0x6e, 0x50, 0x6c, 0x61, 0x79, 0x65, 0x72, 0x73, 0x12, 0x1f, 0x0a, 0x0b,
	0x6d, 0x61, 0x78, 0x5f, 0x70, 0x6c, 0x61, 0x79, 0x65, 0x72, 0x73, 0x18, 0x02, 0x20, 0x01, 0x28,
	0x05, 0x52, 0x0a, 0x6d, 0x61, 0x78, 0x50, 0x6c, 0x61, 0x79, 0x65, 0x72, 0x73, 0x12, 0x12, 0x0a,
	0x04, 0x70, 0x6f, 0x6f, 0x6c, 0x18, 0x03, 0x20, 0x01, 0x28, 0x09, 0x52, 0x04, 0x70, 0x6f, 0x6f,
	0x6c, 0x12, 0x1f, 0x0a, 0x0b, 0x73, 0x6b, 0x69, 0x6c, 0x6c, 0x5f, 0x6c, 0x65, 0x76, 0x65, 0x6c,
	0x18, 0x04, 0x20, 0x01, 0x28, 0x09, 0x52, 0x0a, 0x73, 0x6b, 0x69, 0x6c, 0x6c, 0x4c, 0x65, 0x76,
	0x65, 0x6c, 0x12, 0x2f, 0x0a, 0x13, 0x72, 0x65, 0x71, 0x75, 0x69, 0x72, 0x65, 0x64, 0x5f, 0x70,
	0x72, 0x6f, 0x70, 0x65, 0x72, 0x74, 0x69, 0x65, 0x73, 0x18, 0x05, 0x20, 0x03, 0x28, 0x09, 0x52,
	0x12, 0x72, 0x65, 0x71, 0x75, 0x69, 0x72, 0x65, 0x64, 0x50, 0x72, 0x6f, 0x70, 0x65, 0x72, 0x74,
	0x69, 0x65, 0x73, 0x12, 0x45, 0x0a, 0x07, 0x66, 0x69, 0x6c, 0x74, 0x65, 0x72, 0x73, 0x18, 0x06,
	0x20, 0x03, 0x28, 0x0b, 0x32, 0x2b, 0x2e, 0x67, 0x61, 0x6d, 0x65, 0x63, 0x6c, 0x6f, 0x75, 0x64,
	0x2e, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x6d, 0x61, 0x6b, 0x69, 0x6e, 0x67, 0x43, 0x72, 0x69, 0x74,
	0x65, 0x72, 0x69, 0x61, 0x2e, 0x46, 0x69, 0x6c, 0x74, 0x65, 0x72, 0x73, 0x45, 0x6e, 0x74, 0x72,
	0x79, 0x52, 0x07, 0x66, 0x69, 0x6c, 0x74, 0x65, 0x72, 0x73, 0x1a, 0x3a, 0x0a, 0x0c, 0x46, 0x69,
	0x6c, 0x74, 0x65, 0x72, 0x73, 0x45, 0x6e, 0x74, 0x72, 0x79, 0x12, 0x10, 0x0a, 0x03, 0x6b, 0x65,
	0x79, 0x18, 0x01, 0x20, 0x01, 0x28, 0x09, 0x52, 0x03, 0x6b, 0x65, 0x79, 0x12, 0x14, 0x0a, 0x05,
	0x76, 0x61, 0x6c, 0x75, 0x65, 0x18, 0x02, 0x20, 0x01, 0x28, 0x09, 0x52, 0x05, 0x76, 0x61, 0x6c,
	0x75, 0x65, 0x3a, 0x02, 0x38, 0x01, 0x22, 0xea, 0x03, 0x0a, 0x11, 0x4d, 0x61, 0x74, 0x63, 0x68,
	0x6d, 0x61, 0x6b, 0x69, 0x6e, 0x67, 0x54, 0x69, 0x63, 0x6b, 0x65, 0x74, 0x12, 0x1b, 0x0a, 0x09,
	0x74, 0x69, 0x63, 0x6b, 0x65, 0x74, 0x5f, 0x69, 0x64, 0x18, 0x01, 0x20, 0x01, 0x28, 0x09, 0x52,
	0x08, 0x74, 0x69, 0x63, 0x6b, 0x65, 0x74, 0x49, 0x64, 0x12, 0x17, 0x0a, 0x07, 0x75, 0x73, 0x65,
	0x72, 0x5f, 0x69, 0x64, 0x18, 0x02, 0x20, 0x01, 0x28, 0x09, 0x52, 0x06, 0x75, 0x73, 0x65, 0x72,
	0x49, 0x64, 0x12, 0x17, 0x0a, 0x07, 0x67, 0x61, 0x6d, 0x65, 0x5f, 0x69, 0x64, 0x18, 0x03, 0x20,
	0x01, 0x28, 0x09, 0x52, 0x06, 0x67, 0x61, 0x6d, 0x65, 0x49, 0x64, 0x12, 0x3a, 0x0a, 0x08, 0x63,
	0x72, 0x69, 0x74, 0x65, 0x72, 0x69, 0x61, 0x18, 0x04, 0x20, 0x01, 0x28, 0x0b, 0x32, 0x1e, 0x2e,
	0x67, 0x61, 0x6d, 0x65, 0x63, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x6d,
	0x61, 0x6b, 0x69, 0x6e, 0x67, 0x43, 0x72, 0x69, 0x74, 0x65, 0x72, 0x69, 0x61, 0x52, 0x08, 0x63,
	0x72, 0x69, 0x74, 0x65, 0x72, 0x69, 0x61, 0x12, 0x4c, 0x0a, 0x0a, 0x70, 0x72, 0x6f, 0x70, 0x65,
	0x72, 0x74, 0x69, 0x65, 0x73, 0x18, 0x05, 0x20, 0x03, 0x28, 0x0b, 0x32, 0x2c, 0x2e, 0x67, 0x61,
	0x6d, 0x65, 0x63, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x6d, 0x61, 0x6b,
	0x69, 0x6e, 0x67, 0x54, 0x69, 0x63, 0x6b, 0x65, 0x74, 0x2e, 0x50, 0x72, 0x6f, 0x70, 0x65, 0x72,
	0x74, 0x69, 0x65, 0x73, 0x45, 0x6e, 0x74, 0x72, 0x79, 0x52, 0x0a, 0x70, 0x72, 0x6f, 0x70, 0x65,
	0x72, 0x74, 0x69, 0x65, 0x73, 0x12, 0x2e, 0x0a, 0x06, 0x73, 0x74, 0x61, 0x74, 0x75, 0x73, 0x18,
	0x06, 0x20, 0x01, 0x28, 0x0e, 0x32, 0x16, 0x2e, 0x67, 0x61, 0x6d, 0x65, 0x63, 0x6c, 0x6f, 0x75,
	0x64, 0x2e, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x53, 0x74, 0x61, 0x74, 0x75, 0x73, 0x52, 0x06, 0x73,
	0x74, 0x61, 0x74, 0x75, 0x73, 0x12, 0x17, 0x0a, 0x07, 0x72, 0x6f, 0x6f, 0x6d, 0x5f, 0x69, 0x64,
	0x18, 0x07, 0x20, 0x01, 0x28, 0x09, 0x52, 0x06, 0x72, 0x6f, 0x6f, 0x6d, 0x49, 0x64, 0x12, 0x39,
	0x0a, 0x0a, 0x63, 0x72, 0x65, 0x61, 0x74, 0x65, 0x64, 0x5f, 0x61, 0x74, 0x18, 0x08, 0x20, 0x01,
	0x28, 0x0b, 0x32, 0x1a, 0x2e, 0x67, 0x6f, 0x6f, 0x67, 0x6c, 0x65, 0x2e, 0x70, 0x72, 0x6f, 0x74,
	0x6f, 0x62, 0x75, 0x66, 0x2e, 0x54, 0x69, 0x6d, 0x65, 0x73, 0x74, 0x61, 0x6d, 0x70, 0x52, 0x09,
	0x63, 0x72, 0x65, 0x61, 0x74, 0x65, 0x64, 0x41, 0x74, 0x12, 0x39, 0x0a, 0x0a, 0x75, 0x70, 0x64,
	0x61, 0x74, 0x65, 0x64, 0x5f, 0x61, 0x74, 0x18, 0x09, 0x20, 0x01, 0x28, 0x0b, 0x32, 0x1a, 0x2e,
	0x67, 0x6f, 0x6f, 0x67, 0x6c, 0x65, 0x2e, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x62, 0x75, 0x66, 0x2e,
	0x54, 0x69, 0x6d, 0x65, 0x73, 0x74, 0x61, 0x6d, 0x70, 0x52, 0x09, 0x75, 0x70, 0x64, 0x61, 0x74,
	0x65, 0x64, 0x41, 0x74, 0x1a, 0x3d, 0x0a, 0x0f, 0x50, 0x72, 0x6f, 0x70, 0x65, 0x72, 0x74, 0x69,
	0x65, 0x73, 0x45, 0x6e, 0x74, 0x72, 0x79, 0x12, 0x10, 0x0a, 0x03, 0x6b, 0x65, 0x79, 0x18, 0x01,
	0x20, 0x01, 0x28, 0x09, 0x52, 0x03, 0x6b, 0x65, 0x79, 0x12, 0x14, 0x0a, 0x05, 0x76, 0x61, 0x6c,
	0x75, 0x65, 0x18, 0x02, 0x20, 0x01, 0x28, 0x09, 0x52, 0x05, 0x76, 0x61, 0x6c, 0x75, 0x65, 0x3a,
	0x02, 0x38, 0x01, 0x22, 0x32, 0x0a, 0x13, 0x44, 0x65, 0x6c, 0x65, 0x74, 0x65, 0x54, 0x69, 0x63,
	0x6b, 0x65, 0x74, 0x52, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x12, 0x1b, 0x0a, 0x09, 0x74, 0x69,
	0x63, 0x6b, 0x65, 0x74, 0x5f, 0x69, 0x64, 0x18, 0x01, 0x20, 0x01, 0x28, 0x09, 0x52, 0x08, 0x74,
	0x69, 0x63, 0x6b, 0x65, 0x74, 0x49, 0x64, 0x22, 0x2f, 0x0a, 0x10, 0x47, 0x65, 0x74, 0x54, 0x69,
	0x63, 0x6b, 0x65, 0x74, 0x52, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x12, 0x1b, 0x0a, 0x09, 0x74,
	0x69, 0x63, 0x6b, 0x65, 0x74, 0x5f, 0x69, 0x64, 0x18, 0x01, 0x20, 0x01, 0x28, 0x09, 0x52, 0x08,
	0x74, 0x69, 0x63, 0x6b, 0x65, 0x74, 0x49, 0x64, 0x22, 0x88, 0x01, 0x0a, 0x16, 0x41, 0x64, 0x64,
	0x55, 0x73, 0x65, 0x72, 0x73, 0x54, 0x6f, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x52, 0x65, 0x71, 0x75,
	0x65, 0x73, 0x74, 0x12, 0x19, 0x0a, 0x08, 0x75, 0x73, 0x65, 0x72, 0x5f, 0x69, 0x64, 0x73, 0x18,
	0x01, 0x20, 0x03, 0x28, 0x09, 0x52, 0x07, 0x75, 0x73, 0x65, 0x72, 0x49, 0x64, 0x73, 0x12, 0x17,
	0x0a, 0x07, 0x67, 0x61, 0x6d, 0x65, 0x5f, 0x69, 0x64, 0x18, 0x02, 0x20, 0x01, 0x28, 0x09, 0x52,
	0x06, 0x67, 0x61, 0x6d, 0x65, 0x49, 0x64, 0x12, 0x3a, 0x0a, 0x08, 0x63, 0x72, 0x69, 0x74, 0x65,
	0x72, 0x69, 0x61, 0x18, 0x03, 0x20, 0x01, 0x28, 0x0b, 0x32, 0x1e, 0x2e, 0x67, 0x61, 0x6d, 0x65,
	0x63, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x6d, 0x61, 0x6b, 0x69, 0x6e,
	0x67, 0x43, 0x72, 0x69, 0x74, 0x65, 0x72, 0x69, 0x61, 0x52, 0x08, 0x63, 0x72, 0x69, 0x74, 0x65,
	0x72, 0x69, 0x61, 0x2a, 0xaf, 0x01, 0x0a, 0x0b, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x53, 0x74, 0x61,
	0x74, 0x75, 0x73, 0x12, 0x1c, 0x0a, 0x18, 0x4d, 0x41, 0x54, 0x43, 0x48, 0x5f, 0x53, 0x54, 0x41,
	0x54, 0x55, 0x53, 0x5f, 0x55, 0x4e, 0x53, 0x50, 0x45, 0x43, 0x49, 0x46, 0x49, 0x45, 0x44, 0x10,
	0x00, 0x12, 0x17, 0x0a, 0x13, 0x4d, 0x41, 0x54, 0x43, 0x48, 0x5f, 0x53, 0x54, 0x41, 0x54, 0x55,
	0x53, 0x5f, 0x51, 0x55, 0x45, 0x55, 0x45, 0x44, 0x10, 0x01, 0x12, 0x18, 0x0a, 0x14, 0x4d, 0x41,
	0x54, 0x43, 0x48, 0x5f, 0x53, 0x54, 0x41, 0x54, 0x55, 0x53, 0x5f, 0x4d, 0x41, 0x54, 0x43, 0x48,
	0x45, 0x44, 0x10, 0x02, 0x12, 0x1a, 0x0a, 0x16, 0x4d, 0x41, 0x54, 0x43, 0x48, 0x5f, 0x53, 0x54,
	0x41, 0x54, 0x55, 0x53, 0x5f, 0x43, 0x41, 0x4e, 0x43, 0x45, 0x4c, 0x4c, 0x45, 0x44, 0x10, 0x03,
	0x12, 0x17, 0x0a, 0x13, 0x4d, 0x41, 0x54, 0x43, 0x48, 0x5f, 0x53, 0x54, 0x41, 0x54, 0x55, 0x53,
	0x5f, 0x46, 0x41, 0x49, 0x4c, 0x45, 0x44, 0x10, 0x04, 0x12, 0x1a, 0x0a, 0x16, 0x4d, 0x41, 0x54,
	0x43, 0x48, 0x5f, 0x53, 0x54, 0x41, 0x54, 0x55, 0x53, 0x5f, 0x54, 0x49, 0x4d, 0x45, 0x44, 0x5f,
	0x4f, 0x55, 0x54, 0x10, 0x05, 0x32, 0xc6, 0x02, 0x0a, 0x12, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x6d,
	0x61, 0x6b, 0x69, 0x6e, 0x67, 0x53, 0x65, 0x72, 0x76, 0x69, 0x63, 0x65, 0x12, 0x4c, 0x0a, 0x0c,
	0x43, 0x72, 0x65, 0x61, 0x74, 0x65, 0x54, 0x69, 0x63, 0x6b, 0x65, 0x74, 0x12, 0x1e, 0x2e, 0x67,
	0x61, 0x6d, 0x65, 0x63, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x43, 0x72, 0x65, 0x61, 0x74, 0x65, 0x54,
	0x69, 0x63, 0x6b, 0x65, 0x74, 0x52, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x1a, 0x1c, 0x2e, 0x67,
	0x61, 0x6d, 0x65, 0x63, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x6d, 0x61,
	0x6b, 0x69, 0x6e, 0x67, 0x54, 0x69, 0x63, 0x6b, 0x65, 0x74, 0x12, 0x46, 0x0a, 0x0c, 0x44, 0x65,
	0x6c, 0x65, 0x74, 0x65, 0x54, 0x69, 0x63, 0x6b, 0x65, 0x74, 0x12, 0x1e, 0x2e, 0x67, 0x61, 0x6d,
	0x65, 0x63, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x44, 0x65, 0x6c, 0x65, 0x74, 0x65, 0x54, 0x69, 0x63,
	0x6b, 0x65, 0x74, 0x52, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x1a, 0x16, 0x2e, 0x67, 0x6f, 0x6f,
	0x67, 0x6c, 0x65, 0x2e, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x62, 0x75, 0x66, 0x2e, 0x45, 0x6d, 0x70,
	0x74, 0x79, 0x12, 0x46, 0x0a, 0x09, 0x47, 0x65, 0x74, 0x54, 0x69, 0x63, 0x6b, 0x65, 0x74, 0x12,
	0x1b, 0x2e, 0x67, 0x61, 0x6d, 0x65, 0x63, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x47, 0x65, 0x74, 0x54,
	0x69, 0x63, 0x6b, 0x65, 0x74, 0x52, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x1a, 0x1c, 0x2e, 0x67,
	0x61, 0x6d, 0x65, 0x63, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x6d, 0x61,
	0x6b, 0x69, 0x6e, 0x67, 0x54, 0x69, 0x63, 0x6b, 0x65, 0x74, 0x12, 0x52, 0x0a, 0x0f, 0x41, 0x64,
	0x64, 0x55, 0x73, 0x65, 0x72, 0x73, 0x54, 0x6f, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x12, 0x21, 0x2e,
	0x67, 0x61, 0x6d, 0x65, 0x63, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x41, 0x64, 0x64, 0x55, 0x73, 0x65,
	0x72, 0x73, 0x54, 0x6f, 0x4d, 0x61, 0x74, 0x63, 0x68, 0x52, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74,
	0x1a, 0x1c, 0x2e, 0x67, 0x61, 0x6d, 0x65, 0x63, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x4d, 0x61, 0x74,
	0x63, 0x68, 0x6d, 0x61, 0x6b, 0x69, 0x6e, 0x67, 0x54, 0x69, 0x63, 0x6b, 0x65, 0x74, 0x42, 0x42,
	0x5a, 0x2e, 0x67, 0x69, 0x74, 0x68, 0x75, 0x62, 0x2e, 0x63, 0x6f, 0x6d, 0x2f, 0x74, 0x75, 0x72,
	0x61, 0x6e, 0x68, 0x65, 0x79, 0x64, 0x61, 0x72, 0x6c, 0x69, 0x2f, 0x67, 0x61, 0x6d, 0x65, 0x63,
	0x6c, 0x6f, 0x75, 0x64, 0x2f, 0x72, 0x65, 0x6c, 0x61, 0x79, 0x2f, 0x70, 0x72, 0x6f, 0x74, 0x6f,
	0xaa, 0x02, 0x0f, 0x47, 0x61, 0x6d, 0x65, 0x43, 0x6c, 0x6f, 0x75, 0x64, 0x2e, 0x50, 0x72, 0x6f,
	0x74, 0x6f, 0x62, 0x06, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x33,
}

var (
	file_matchmaking_proto_rawDescOnce sync.Once
	file_matchmaking_proto_rawDescData = file_matchmaking_proto_rawDesc
)

func file_matchmaking_proto_rawDescGZIP() []byte {
	file_matchmaking_proto_rawDescOnce.Do(func() {
		file_matchmaking_proto_rawDescData = protoimpl.X.CompressGZIP(file_matchmaking_proto_rawDescData)
	})
	return file_matchmaking_proto_rawDescData
}

var file_matchmaking_proto_enumTypes = make([]protoimpl.EnumInfo, 1)
var file_matchmaking_proto_msgTypes = make([]protoimpl.MessageInfo, 9)
var file_matchmaking_proto_goTypes = []interface{}{
	(MatchStatus)(0),               // 0: gamecloud.MatchStatus
	(*CreateTicketRequest)(nil),    // 1: gamecloud.CreateTicketRequest
	(*MatchmakingCriteria)(nil),    // 2: gamecloud.MatchmakingCriteria
	(*MatchmakingTicket)(nil),      // 3: gamecloud.MatchmakingTicket
	(*DeleteTicketRequest)(nil),    // 4: gamecloud.DeleteTicketRequest
	(*GetTicketRequest)(nil),       // 5: gamecloud.GetTicketRequest
	(*AddUsersToMatchRequest)(nil), // 6: gamecloud.AddUsersToMatchRequest
	nil,                            // 7: gamecloud.CreateTicketRequest.PropertiesEntry
	nil,                            // 8: gamecloud.MatchmakingCriteria.FiltersEntry
	nil,                            // 9: gamecloud.MatchmakingTicket.PropertiesEntry
	(*timestamppb.Timestamp)(nil),  // 10: google.protobuf.Timestamp
	(*emptypb.Empty)(nil),          // 11: google.protobuf.Empty
}
var file_matchmaking_proto_depIdxs = []int32{
	2,  // 0: gamecloud.CreateTicketRequest.criteria:type_name -> gamecloud.MatchmakingCriteria
	7,  // 1: gamecloud.CreateTicketRequest.properties:type_name -> gamecloud.CreateTicketRequest.PropertiesEntry
	8,  // 2: gamecloud.MatchmakingCriteria.filters:type_name -> gamecloud.MatchmakingCriteria.FiltersEntry
	2,  // 3: gamecloud.MatchmakingTicket.criteria:type_name -> gamecloud.MatchmakingCriteria
	9,  // 4: gamecloud.MatchmakingTicket.properties:type_name -> gamecloud.MatchmakingTicket.PropertiesEntry
	0,  // 5: gamecloud.MatchmakingTicket.status:type_name -> gamecloud.MatchStatus
	10, // 6: gamecloud.MatchmakingTicket.created_at:type_name -> google.protobuf.Timestamp
	10, // 7: gamecloud.MatchmakingTicket.updated_at:type_name -> google.protobuf.Timestamp
	2,  // 8: gamecloud.AddUsersToMatchRequest.criteria:type_name -> gamecloud.MatchmakingCriteria
	1,  // 9: gamecloud.MatchmakingService.CreateTicket:input_type -> gamecloud.CreateTicketRequest
	4,  // 10: gamecloud.MatchmakingService.DeleteTicket:input_type -> gamecloud.DeleteTicketRequest
	5,  // 11: gamecloud.MatchmakingService.GetTicket:input_type -> gamecloud.GetTicketRequest
	6,  // 12: gamecloud.MatchmakingService.AddUsersToMatch:input_type -> gamecloud.AddUsersToMatchRequest
	3,  // 13: gamecloud.MatchmakingService.CreateTicket:output_type -> gamecloud.MatchmakingTicket
	11, // 14: gamecloud.MatchmakingService.DeleteTicket:output_type -> google.protobuf.Empty
	3,  // 15: gamecloud.MatchmakingService.GetTicket:output_type -> gamecloud.MatchmakingTicket
	3,  // 16: gamecloud.MatchmakingService.AddUsersToMatch:output_type -> gamecloud.MatchmakingTicket
	13, // [13:17] is the sub-list for method output_type
	9,  // [9:13] is the sub-list for method input_type
	9,  // [9:9] is the sub-list for extension type_name
	9,  // [9:9] is the sub-list for extension extendee
	0,  // [0:9] is the sub-list for field type_name
}

func init() { file_matchmaking_proto_init() }
func file_matchmaking_proto_init() {
	if File_matchmaking_proto != nil {
		return
	}
	if !protoimpl.UnsafeEnabled {
		file_matchmaking_proto_msgTypes[0].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*CreateTicketRequest); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_matchmaking_proto_msgTypes[1].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*MatchmakingCriteria); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_matchmaking_proto_msgTypes[2].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*MatchmakingTicket); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_matchmaking_proto_msgTypes[3].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*DeleteTicketRequest); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_matchmaking_proto_msgTypes[4].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*GetTicketRequest); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_matchmaking_proto_msgTypes[5].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*AddUsersToMatchRequest); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
	}
	type x struct{}
	out := protoimpl.TypeBuilder{
		File: protoimpl.DescBuilder{
			GoPackagePath: reflect.TypeOf(x{}).PkgPath(),
			RawDescriptor: file_matchmaking_proto_rawDesc,
			NumEnums:      1,
			NumMessages:   9,
			NumExtensions: 0,
			NumServices:   1,
		},
		GoTypes:           file_matchmaking_proto_goTypes,
		DependencyIndexes: file_matchmaking_proto_depIdxs,
		EnumInfos:         file_matchmaking_proto_enumTypes,
		MessageInfos:      file_matchmaking_proto_msgTypes,
	}.Build()
	File_matchmaking_proto = out.File
	file_matchmaking_proto_rawDesc = nil
	file_matchmaking_proto_goTypes = nil
	file_matchmaking_proto_depIdxs = nil
}
