using GameCloud.Domain.Entities;
using GameCloud.Domain.Enums;
using GameCloud.Domain.Repositories;
using GameCloud.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GameCloud.Persistence.Repositories;

public class ImageDocumentRepository(GameCloudDbContext context) : IImageDocumentRepository
{
    public async Task<ImageDocument> CreateAsync(ImageDocument imageDocument)
    {
        context.Entry(imageDocument).State = EntityState.Added;
        await context.SaveChangesAsync();
        return imageDocument;
    }

    public async Task<ImageDocument> UpdateAsync(ImageDocument imageDocument)
    {
        context.Entry(imageDocument).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return imageDocument;
    }

    public async Task<ImageDocument?> GetByIdAsync(Guid id)
    {
        IQueryable<ImageDocument?> queryable = context.Set<ImageDocument>();

        queryable = queryable.Where(document => document != null && document.Id == id)
            .Include(v => v.Variants);

        return await queryable.FirstOrDefaultAsync();
    }

    public async Task<ImageDocument?> GetByIdAsync(Guid id, ImageType type)
    {
        IQueryable<ImageDocument?> queryable = context.Set<ImageDocument>();

        queryable = queryable.Where(document => document != null
                                                && document.Id == id
                                                && document.Type == type);

        queryable = queryable.Include(document => document.Variants);

        return await queryable.FirstOrDefaultAsync();
    }
}