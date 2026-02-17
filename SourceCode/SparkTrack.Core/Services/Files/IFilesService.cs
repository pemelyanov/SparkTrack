namespace SparkTrack.Core.Services.Files;

public interface IFilesService
{
    Task<Guid> UploadAsync(Stream stream, long contentLength, CancellationToken cancellationToken);

    Task DownloadAsync(Guid id, Stream stream, CancellationToken cancellationToken, Action<long> contentLengthCallback);

    Task DeleteAsync(Guid id);
}