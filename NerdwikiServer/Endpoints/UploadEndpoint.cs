using NerdwikiServer.Extensions;

namespace NerdwikiServer.Endpoints;

public static class UploadEndpoint
{
    public static WebApplication MapUploadApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/upload");

        group.MapGet("/{fileName}", GetImage);
        group.MapPost("/", UploadImage);

        return app;
    }

    public static async Task<IResult> GetImage(string fileName)
    {
        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "FileStorage", fileName);
        if (!File.Exists(imagePath))
            return TypedResults.NotFound();

        var fileBytes = await File.ReadAllBytesAsync(imagePath);
        var contentType = GetContentType(imagePath);

        return TypedResults.File(fileBytes, contentType);
    }

    public static async Task<IResult> UploadImage(HttpRequest request)
    {
        try
        {
            var form = await request.ReadFormAsync();
            var files = form.Files;
            var file = files.Any() && files.Count > 0 ? files[0] : null;
            if (file is null || file.Length == 0)
                return TypedResults.BadRequest("No file uploaded");

            var fileName = file.B64UrlHashName();
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "FileStorage");
            var filePath = Path.Combine(uploadPath, fileName);

            var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return TypedResults.Ok(fileName);
        }
        catch (Exception e)
        {
            return TypedResults.BadRequest(e.Message);
        }
    }

    private static string GetContentType(string fileName)
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