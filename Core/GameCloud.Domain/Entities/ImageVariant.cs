namespace GameCloud.Domain.Entities;

public class ImageVariant : BaseEntity
{
    public string Url { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string VariantType { get; set; }
    public Guid ImageDocumentId { get; set; }
}