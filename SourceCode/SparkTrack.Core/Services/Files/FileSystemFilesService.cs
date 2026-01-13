namespace SparkTrack.Core.Services.Files;

public class FileSystemFilesService : IFilesService
{
    private const string FilesFolder = "UploadedFiles";

    public async Task<Guid> UploadAsync(Stream stream)
    {
        Directory.CreateDirectory(FilesFolder);
        
        var fileId = Guid.CreateVersion7();
        await using var fileStream = File.OpenWrite(Path.Combine(FilesFolder, fileId.ToString()));
        await stream.CopyToAsync(fileStream);

        return fileId;
    }

    public Task<Stream?> DownloadAsync(Guid id)
    {
        var filePath = Path.Combine(FilesFolder, id.ToString());

        if (!File.Exists(filePath)) return Task.FromResult<Stream?>(null);

        return Task.FromResult<Stream?>(File.OpenRead(filePath));
    }

    public Task DeleteAsync(Guid id)
    {
        var filePath = Path.Combine(FilesFolder, id.ToString());

        if (!File.Exists(filePath)) return Task.CompletedTask;
        
        File.Delete(filePath);
        
        return Task.CompletedTask;
    }
}