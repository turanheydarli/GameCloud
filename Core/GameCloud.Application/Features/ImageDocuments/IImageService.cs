using GameCloud.Application.Features.ImageDocuments.Requests;
using GameCloud.Application.Features.ImageDocuments.Responses;

namespace GameCloud.Application.Features.ImageDocuments;

public interface IImageService
{
    Task<ImageResponse> UploadAsync(ImageUploadRequest request);
    Task<ImageResponse> GetByIdAsync(Guid imageId);
    Task<bool> DeleteAsync(Guid imageId);
}