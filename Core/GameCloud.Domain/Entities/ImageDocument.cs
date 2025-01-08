using GameCloud.Domain.Enums;

namespace GameCloud.Domain.Entities;

public class ImageDocument : BaseEntity
{
    public string Url { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string FileType { get; set; }
    public string StorageProvider { get; set; }
    public ImageType Type { get; set; }
    public virtual ICollection<ImageVariant> Variants { get; set; } = new List<ImageVariant>();
}