using Microsoft.AspNetCore.Http;

namespace Application.SubcutaneousTests.Extensions;

public static class FileHelper
{
    public static IFormFile GenerateIFormFile(string filePath)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        using MemoryStream memoryStream = new(fileBytes);
        IFormFile formFile = new FormFile(
            memoryStream,
            0,
            fileBytes.Length,
            Path.GetFileName(filePath),
            Path.GetFileName(filePath)
        );

        return formFile;
    }
}
