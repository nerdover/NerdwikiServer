using System.Security.Cryptography;

namespace NerdwikiServer.Extensions;

public static class FileHasher
{
    public static string B64UrlHashName(this IFormFile file)
    {
        var hash = SHA256.Create().ComputeHash(file.OpenReadStream());
        string fileName = Convert.ToBase64String(hash)
            .TrimEnd('=')
            .Replace("/", "-")
            .Replace("+", "-");
        fileName += "." + file.FileName.Split(".").Last();

        return fileName;
    }
}
