namespace SparkTrack.Core.Services.Files;

public interface IFilesService
{
    Task<Guid> UploadAsync(Stream stream);

    Task<Stream?> DownloadAsync(Guid id);

    Task DeleteAsync(Guid id);
}