namespace SparkTrack.Core.Client.Services.Files;

using Data;

public interface IFilesService
{
    Task<Guid> UploadAsync(Stream stream, LoadingProgress progress);
    
    Task DownloadAsync(Guid id, string outputPath, LoadingProgress progress);
}