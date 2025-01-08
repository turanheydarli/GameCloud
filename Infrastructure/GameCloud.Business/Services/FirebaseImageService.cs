using Google.Cloud.Storage.V1;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Options;
using GameCloud.Application.Features.ImageDocuments;
using GameCloud.Application.Features.ImageDocuments.Requests;
using GameCloud.Application.Features.ImageDocuments.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;
using GameCloud.Domain.Repositories;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace GameCloud.Business.Services;

public class FirebaseStorageService(
    IOptions<FirebaseStorageOptions> options,
    IImageDocumentRepository imageDocumentRepository) : IImageService
{
    private readonly StorageClient _storageClient = StorageClient.Create();
    private readonly FirebaseStorageOptions _options = options.Value;

    private readonly Dictionary<ImageType, (int width, int height)[]> _imageSizes = new()
    {
        [ImageType.GameIcon] = new[]
        {
            (400, 400), // Original
            (200, 200), // Medium
            (100, 100) // Thumbnail
        },
        [ImageType.DeveloperProfile] = new[]
        {
            (400, 400), // Original
            (200, 200), // Medium
            (100, 100) // Thumbnail
        }
    };

    public async Task<ImageResponse> UploadAsync(ImageUploadRequest request)
    {
        using var image = await Image.LoadAsync(request.ImageStream);
        var variants = new List<ImageVariant>();

        var sizes = _imageSizes[request.Type];
        var fileExtension = Path.GetExtension(request.FileName);
        var folderPath = $"{request.Type.ToString().ToLower()}/{Guid.NewGuid()}";
        var mainImagePath = $"{folderPath}/original{fileExtension}";

        var mainImageUrl = await UploadImageToStorage(image, mainImagePath, request.ContentType);

        foreach (var (width, height) in sizes.Skip(1))
        {
            using var resized = image.Clone(ctx =>
            {
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = ResizeMode.Max
                });
            });

            var variantPath = $"{folderPath}/{width}x{height}{fileExtension}";
            var variantUrl = await UploadImageToStorage(resized, variantPath, request.ContentType);

            variants.Add(new ImageVariant
            {
                Url = variantUrl,
                Width = width,
                Height = height,
                VariantType = width == sizes[1].width ? "medium" : "thumbnail"
            });
        }

        var imageDocument = new ImageDocument
        {
            Url = mainImageUrl,
            Width = sizes[0].width,
            Height = sizes[0].height,
            FileType = request.ContentType,
            StorageProvider = "Firebase",
            Type = request.Type,
            CreatedAt = DateTime.UtcNow
        };

        imageDocument = await imageDocumentRepository.CreateAsync(imageDocument);
        
        imageDocument.Variants = variants;
        
        imageDocument = await imageDocumentRepository.UpdateAsync(imageDocument);
        
        return ToImageResponse(imageDocument);
    }

    public async Task<bool> DeleteAsync(Guid imageId)
    {
        try
        {
            var image = await GetByIdAsync(imageId);

            await DeleteFileFromStorage(image.Url);

            foreach (var variant in image.Variants)
            {
                await DeleteFileFromStorage(variant.Url);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<ImageResponse> GetByIdAsync(Guid imageId)
    {
        var image = await imageDocumentRepository.GetByIdAsync(imageId);

        return ToImageResponse(image);
    }

    private async Task<string> UploadImageToStorage(Image image, string path, string contentType)
    {
        using var ms = new MemoryStream();
        if (image.Metadata.DecodedImageFormat != null)
            await image.SaveAsync(ms, image.Metadata.DecodedImageFormat);
        ms.Position = 0;

        var obj = await _storageClient.UploadObjectAsync(new Object()
        {
            Bucket = _options.BucketName,
            Name = path,
            ContentType = contentType
        }, ms);

        return $"https://storage.googleapis.com/{_options.BucketName}/{obj.Name}";
    }

    private async Task DeleteFileFromStorage(string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath.TrimStart('/');
            await _storageClient.DeleteObjectAsync(_options.BucketName, path);
        }
        catch
        {
            // Log error but don't throw - file might already be deleted
        }
    }

    private static ImageResponse ToImageResponse(ImageDocument document)
    {
        return new ImageResponse(
            Id: document.Id,
            Url: document.Url,
            Width: document.Width,
            Height: document.Height,
            FileType: document.FileType,
            StorageProvider: document.StorageProvider,
            Type: document.Type,
            Variants: document.Variants.ToList(),
            CreatedAt: document.CreatedAt
        );
    }
}

public class FirebaseStorageOptions
{
    public string BucketName { get; set; } = string.Empty;
}