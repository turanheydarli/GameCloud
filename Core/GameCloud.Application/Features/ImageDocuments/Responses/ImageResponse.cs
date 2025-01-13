using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;

namespace GameCloud.Application.Features.ImageDocuments.Responses;

public record ImageResponse(
    Guid Id,
    string Url,
    int Width,
    int Height,
    string FileType,
    string StorageProvider,
    ImageType Type,
    List<ImageVariant> Variants,
    DateTime CreatedAt);

public record ImageFileResponse 
{
    public required Stream Stream { get; set; }
    public required string ContentType { get; set; }
    public string? FileName { get; set; }

}