using GameCloud.Application.Features.ImageDocuments.Requests;
using GameCloud.Application.Features.ImageDocuments.Responses;

namespace GameCloud.Application.Features.ImageDocuments;

public interface IImageService
{
    Task<ImageResponse> UploadAsync(ImageUploadRequest request);
    Task<bool> DeleteAsync(Guid imageId);
    Task<ImageResponse> GetByIdAsync(Guid imageId);
    Task<ImageFileResponse> GetImageFileAsync(string imageUrl);
    Task<ImageFileResponse> GetImageFileByIdAsync(Guid imageId, string variant = "original");
}