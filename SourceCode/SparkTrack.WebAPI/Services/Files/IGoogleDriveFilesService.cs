namespace SparkTrack.WebAPI.Services.Files;

using Core.Services.Files;

public interface IGoogleDriveFilesService : IFilesService
{
    Task UploadAsync(
        Guid existingId,
        Stream stream,
        long contentLength,
        string? extension,
        CancellationToken cancellationToken
    );
}