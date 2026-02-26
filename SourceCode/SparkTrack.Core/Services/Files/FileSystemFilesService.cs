namespace SparkTrack.Core.Services.Files;

public class FileSystemFilesService : IFilesService
{
    private const string FilesFolder = "UploadedFiles";

    public Task<string> GetLinkAsync(Guid id) => Task.FromResult(string.Empty);

    public async Task<Guid> UploadAsync(Stream stream, long contentLength, string? extension, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(FilesFolder);
        
        var fileId = Guid.CreateVersion7();
        await using var fileStream = File.OpenWrite(Path.Combine(FilesFolder, fileId.ToString()));
        await stream.CopyToAsync(fileStream, cancellationToken);

        return fileId;
    }

    public async Task DownloadAsync(Guid id, Stream stream, CancellationToken cancellationToken, Action<long> contentLengthCallback)
    {
        var filePath = Path.Combine(FilesFolder, id.ToString());

        if (!File.Exists(filePath)) return;

        var fileInfo = new FileInfo(filePath);

        contentLengthCallback(fileInfo.Length);

        await using var fileStream = File.OpenRead(filePath);

        await fileStream.CopyToAsync(stream, cancellationToken);
    }

    public Task DeleteAsync(Guid id)
    {
        var filePath = Path.Combine(FilesFolder, id.ToString());

        if (!File.Exists(filePath)) return Task.CompletedTask;
        
        File.Delete(filePath);
        
        return Task.CompletedTask;
    }
}