using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _basePath;

    public FileService(IWebHostEnvironment env)
    {
        _basePath = Path.Combine(env.WebRootPath, "uploads", "avatars");
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
        
        var extension = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_basePath, uniqueName);
        using (var file = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(file);
        }
        
        return $"/uploads/avatars/{uniqueName}";
    }

    public void Delete(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        var fullPath = Path.Combine(_basePath, Path.GetFileName(filePath));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
