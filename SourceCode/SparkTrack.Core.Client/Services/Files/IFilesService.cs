namespace SparkTrack.Core.Client.Services.Files;

using Data;

public interface IFilesService
{
    Task<Guid> UploadAsync(byte[] content, LoadingProgress progress, CancellationToken cancellationToken = default);
    
    Task<Guid> UploadAsync(string inputPath, LoadingProgress progress, CancellationToken cancellationToken = default);
    
    Task DownloadAsync(Guid id, string outputPath, LoadingProgress progress, CancellationToken  cancellationToken = default);
}