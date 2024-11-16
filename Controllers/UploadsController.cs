using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NerdwikiServer.Data;
using NerdwikiServer.Extensions;

namespace NerdwikiServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UploadsController : ControllerBase
{
    public record UploadResponse(bool IsSuccess, string FileName);

    [HttpGet("{fileName}")]
    public IActionResult GetImage(string fileName)
    {
        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "FileStorage", fileName);
        if (!System.IO.File.Exists(imagePath))
        {
            return NotFound();
        }

        var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        var contentType = GetContentType(imagePath);

        return new FileStreamResult(fileStream, contentType);
    }

    [HttpPost]
    public async Task<ActionResult<UploadResponse>> Upload(IFormFile image)
    {
        try
        {
            if (!image.ContentType.StartsWith("image/"))
            {
                throw new Exception("Upload file is not an image");
            }

            string fileName = image.B64UrlHashName();

            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), "FileStorage");
            using Stream fileStream = new FileStream($"{pathToSave}/{fileName}", FileMode.Create);
            await image.CopyToAsync(fileStream);

            return new UploadResponse(true, fileName);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    protected string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
    }
}
