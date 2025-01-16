using System.Collections.Concurrent;
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
            (400, 400), 
            (200, 200), 
            (100, 100) 
        },
        [ImageType.DeveloperProfile] = new[]
        {
            (400, 400), 
            (200, 200), 
            (100, 100) 
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

    /// TODO: seperate image resizing to different threads
    public async Task<ImageResponse> UploadAsync(ImageUploadRequest request)
{
     var originalImage = await Image.LoadAsync(request.ImageStream);
    
    var fileExtension = Path.GetExtension(request.FileName);
    var folderPath = $"{request.Type.ToString().ToLower()}/{Guid.NewGuid()}";
    var imageSizes = _imageSizes[request.Type];

    var mainImagePath = $"{folderPath}/original{fileExtension}";
    var mainImageUploadTask = UploadImageToStorage(originalImage, mainImagePath, request.ContentType);

    var variantSizes = imageSizes.Skip(1).ToList();
    var resizedImages = new ConcurrentDictionary<(int width, int height), Image>();
    var resizingTasks = new List<Task>();

    foreach (var size in variantSizes)
    {
        var task = Task.Run(() =>
        {
            var resizedImage = originalImage.Clone(ctx =>
            {
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(size.width, size.height),
                    Mode = ResizeMode.Max
                });
            });
            resizedImages[size] = resizedImage;
        });
        resizingTasks.Add(task);
    }

    await Task.WhenAll(resizingTasks);

    var uploadTasks = new List<Task<ImageVariant>>();
    
    foreach (var size in variantSizes)
    {
        var resizedImage = resizedImages[size];
        var variantPath = $"{folderPath}/{size.width}x{size.height}{fileExtension}";
        
        var uploadTask = UploadImageToStorage(resizedImage, variantPath, request.ContentType)
            .ContinueWith(uploadResult => new ImageVariant
            {
                Url = uploadResult.Result,
                Width = size.width,
                Height = size.height,
                VariantType = size == variantSizes[0] ? "medium" : "thumbnail"
            });
            
        uploadTasks.Add(uploadTask);
    }

    var mainImageUrl = await mainImageUploadTask;
    var variants = await Task.WhenAll(uploadTasks);

    originalImage.Dispose();

    foreach (var resizedImage in resizedImages.Values)
    {
        resizedImage.Dispose();
    }

    var imageDocument = new ImageDocument
    {
        Url = mainImageUrl,
        Width = imageSizes[0].width,
        Height = imageSizes[0].height,
        FileType = request.ContentType,
        StorageProvider = "Yandex",
        Type = request.Type,
        CreatedAt = DateTime.UtcNow,
        Variants = variants.ToList()
    };

    imageDocument = await _imageDocumentRepository.CreateAsync(imageDocument);
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