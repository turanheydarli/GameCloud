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