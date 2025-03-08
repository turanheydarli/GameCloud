using GameCloud.Application.Common.Paging;
using GameCloud.Application.Common.Responses;
using GameCloud.Application.Features.Rooms.Requests;
using GameCloud.Application.Features.Rooms.Responses;

namespace GameCloud.Application.Features.Rooms;

public interface IRoomService
{
    Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request);
    Task<RoomResponse> GetRoomAsync(Guid roomId);
    Task<PageableListResponse<RoomResponse>> GetRoomsAsync(Guid gameId, PageableRequest request);
    Task<RoomResponse> UpdateRoomStateAsync(UpdateRoomStateRequest request);
    Task<JoinRoomResponse> JoinRoomAsync(JoinRoomRequest request);
    Task<bool> LeaveRoomAsync(LeaveRoomRequest request);
    Task<bool> KickPlayerAsync(KickPlayerRequest request);
    Task<bool> DeleteRoomAsync(Guid roomId);
    Task<RoomResponse> UpdateRoomMetadataAsync(Guid roomId, Dictionary<string, string> metadata);
    Task<bool> PersistGameStateAsync(PersistGameStateRequest request);
} 