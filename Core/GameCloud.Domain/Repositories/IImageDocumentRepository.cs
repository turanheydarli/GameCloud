using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;

namespace GameCloud.Domain.Repositories;

public interface IImageDocumentRepository
{
    Task<ImageDocument?> GetByIdAsync(Guid id);
    Task<ImageDocument?> GetByIdAsync(Guid id, ImageType type);
    Task<ImageDocument> CreateAsync(ImageDocument imageDocument);
    Task<ImageDocument> UpdateAsync(ImageDocument imageDocument);
}