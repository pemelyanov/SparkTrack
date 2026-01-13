namespace SparkTrack.API.Services.Files;

using Core.Client.Data;
using Core.Client.Services.Files;
using Core.Client.Streams;
using Delegates;

public class FilesService(ClientFactory<FilesClient> clientFactory) : IFilesService
{
    public async Task<Guid> UploadAsync(string inputPath, LoadingProgress progress)
    {
        using var wrapper = clientFactory.Invoke();

        await using var stream = File.OpenRead(inputPath);
        await using var progressStream = new ProgressReadStream(stream, progress);

        return await wrapper.Client.UploadAsync(new FileParameter(progressStream));
    }

    public async Task DownloadAsync(Guid id, string outputPath, LoadingProgress progress)
    {
        using var wrapper = clientFactory.Invoke();

        var response = await wrapper.Client.DownloadAsync(id);

        var length = response.Headers["Content-Length"].Select(long.Parse).First();

        progress.TotalProgress.OnNext(length);

        var outputFolder = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(outputFolder)) Directory.CreateDirectory(outputFolder);

        var fileStream = File.OpenWrite(outputPath);

        await using var progressStream = new ProgressWriteStream(fileStream, progress);

        await response.Stream.CopyToAsync(progressStream);
    }
}