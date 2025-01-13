using GameCloud.Application.Features.ImageDocuments;
using Microsoft.AspNetCore.Mvc;

namespace GameCloud.WebAPI.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class ImagesController(IImageService imageService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetImage(Guid id, [FromQuery] string? variant = null)
    {
        var imageFile = await imageService.GetImageFileByIdAsync(id, variant ?? "original");
        return File(imageFile.Stream, imageFile.ContentType);
    }

    [HttpGet("{*fullPath}")]
    public async Task<IActionResult> GetImageByFullPath(string fullPath)
    {
        var path = Uri.UnescapeDataString(fullPath);
        var imageFile = await imageService.GetImageFileAsync(path);
        return File(imageFile.Stream, imageFile.ContentType);
    }
}