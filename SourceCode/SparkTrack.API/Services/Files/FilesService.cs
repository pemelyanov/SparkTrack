namespace SparkTrack.API.Services.Files;

using System.Net.Http.Headers;
using Core.Client.Data;
using Core.Client.Services.Files;
using Core.Client.Streams;
using Delegates;

public class FilesService(ClientFactory<FilesClient> clientFactory, Func<HttpClient> httpClientFactory) : IFilesService
{
    public async Task<Guid> UploadAsync(string inputPath, LoadingProgress progress)
    {
        await using var stream = File.OpenRead(inputPath);
        await using var progressStream = new ProgressReadStream(stream, progress);
        
        using var content = new StreamContent(progressStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var httpClient = httpClientFactory();
        
        var response = await httpClient.PostAsync("/files", content);
        var id = await response.Content.ReadAsStringAsync();

        return Guid.Parse(id.Trim('"'));
    }

    public async Task DownloadAsync(Guid id, string outputPath, LoadingProgress progress)
    {
        using var wrapper = clientFactory.Invoke();

        var response = await wrapper.Client.DownloadAsync(id);

        var length = response.Headers["Content-Length"].Select(long.Parse).First();

        progress.TotalProgress.OnNext(length);

        var outputFolder = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(outputFolder)) Directory.CreateDirectory(outputFolder);

        var fileStream = File.Open(outputPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

        await using var progressStream = new ProgressWriteStream(fileStream, progress);

        await response.Stream.CopyToAsync(progressStream);
    }
}