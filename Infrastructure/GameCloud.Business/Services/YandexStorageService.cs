using Amazon.S3;
using Amazon.S3.Model;
using GameCloud.Application.Exceptions;
using Microsoft.Extensions.Options;
using GameCloud.Application.Features.ImageDocuments;
using GameCloud.Application.Features.ImageDocuments.Requests;
using GameCloud.Application.Features.ImageDocuments.Responses;
using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;
using GameCloud.Domain.Repositories;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace GameCloud.Business.Services;

public class YandexStorageService : IImageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IImageDocumentRepository _imageDocumentRepository;
    private readonly YandexStorageOptions _options;

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

    public YandexStorageService(
        IAmazonS3 s3Client,
        IOptions<YandexStorageOptions> options,
        IImageDocumentRepository imageDocumentRepository)
    {
        _s3Client = s3Client;
        _imageDocumentRepository = imageDocumentRepository;
        _options = options.Value;
    }

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
            StorageProvider = "Yandex",
            Type = request.Type,
            CreatedAt = DateTime.UtcNow
        };

        imageDocument = await _imageDocumentRepository.CreateAsync(imageDocument);
        
        imageDocument.Variants = variants;
        
        imageDocument = await _imageDocumentRepository.UpdateAsync(imageDocument);
        
        return ToImageResponse(imageDocument);
    }

    private async Task<string> UploadImageToStorage(Image image, string path, string contentType)
    {
        using var ms = new MemoryStream();
        if (image.Metadata.DecodedImageFormat != null)
            await image.SaveAsync(ms, image.Metadata.DecodedImageFormat);
        ms.Position = 0;

        var putRequest = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = path,
            InputStream = ms,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(putRequest);

        return path;
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

            // await _imageDocumentRepository.DeleteAsync(imageId);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<ImageResponse> GetByIdAsync(Guid imageId)
    {
        var image = await _imageDocumentRepository.GetByIdAsync(imageId);
        return ToImageResponse(image);
    }

    public async Task<ImageFileResponse> GetImageFileAsync(string path)
    {
        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _options.BucketName,
                Key = path
            };

            using var response = await _s3Client.GetObjectAsync(getRequest);
            var stream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(stream);
            stream.Position = 0;

            return new ImageFileResponse
            {
                Stream = stream,
                ContentType = response.Headers.ContentType,
                FileName = Path.GetFileName(path)
            };
        }
        catch (Exception ex)
        {
            throw new ImageNotFoundException($"Image not found at path: {path}", ex);
        }
    }

    public async Task<ImageFileResponse> GetImageFileByIdAsync(Guid imageId, string variant = "original")
    {
        var image = await _imageDocumentRepository.GetByIdAsync(imageId);
        if (image == null)
            throw new ImageNotFoundException($"Image with ID {imageId} not found");

        string url;
        if (variant == "original")
        {
            url = image.Url;
        }
        else
        {
            var variantImage = image.Variants.FirstOrDefault(v => v.VariantType == variant);
            if (variantImage == null)
                throw new ImageVariantNotFoundException($"Variant {variant} not found for image {imageId}");
            url = variantImage.Url;
        }

        return await GetImageFileAsync(url);
    }

    private async Task DeleteFileFromStorage(string path)
    {
        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = path
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
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

public class YandexStorageOptions
{
    public string BucketName { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ServiceUrl { get; set; } = string.Empty;
}