using GameCloud.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace GameCloud.Application.Features.ImageDocuments.Requests;

public record ImageUploadRequest
{
    public required Stream ImageStream { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required ImageType Type { get; set; }
}