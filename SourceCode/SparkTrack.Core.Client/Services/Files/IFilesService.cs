namespace SparkTrack.Core.Client.Services.Files;

using Data;

public interface IFilesService
{
    Task<Guid> UploadAsync(string inputPath, LoadingProgress progress);
    
    Task DownloadAsync(Guid id, string outputPath, LoadingProgress progress);
}