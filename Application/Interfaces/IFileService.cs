namespace Application.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(Stream file, string fileName, string subFolder);
    void Delete(string filePath);
}
