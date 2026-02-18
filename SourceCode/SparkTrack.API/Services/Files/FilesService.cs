namespace SparkTrack.API.Services.Files;

using System.Net.Http.Headers;
using Core.Client.Data;
using Core.Client.Services.Files;
using Core.Client.Streams;
using Delegates;

public class FilesService(CustomClientFactory<FilesClient> clientFactory, Func<HttpClient> httpClientFactory) : IFilesService
{
    public async Task<Guid> UploadAsync(
        byte[] content,
        LoadingProgress progress,
        CancellationToken cancellationToken = default
    )
    {
        using var stream = new MemoryStream(content);
        return await UploadAsync(progress, cancellationToken, stream);
    }

    public async Task<Guid> UploadAsync(string inputPath, LoadingProgress progress, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(inputPath);
        return await UploadAsync(progress, cancellationToken, stream);
    }

    public async Task DownloadAsync(
        Guid id,
        string outputPath,
        LoadingProgress progress,
        CancellationToken cancellationToken
    )
    {
        using var wrapper = clientFactory.Invoke(GetConfiguredHttpClient());

        var response = await wrapper.Client.DownloadAsync(id, cancellationToken);

        var length = response.Headers["Content-Length"].Select(long.Parse).First();

        progress.TotalProgress.OnNext(length);

        var outputFolder = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(outputFolder)) Directory.CreateDirectory(outputFolder);

        var fileStream = File.Open(outputPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

        await using var progressStream = new ProgressWriteStream(fileStream, progress);

        await response.Stream.CopyToAsync(progressStream, cancellationToken);
    }
    
    private async Task<Guid> UploadAsync(LoadingProgress progress, CancellationToken cancellationToken, Stream stream)
    {
        await using var progressStream = new ProgressReadStream(stream, progress);

        using var content = new StreamContent(progressStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Headers.ContentLength = stream.Length;

        using HttpClient httpClient = GetConfiguredHttpClient();

        var response = await httpClient.PostAsync("/files", content, cancellationToken);
        var id = await response.Content.ReadAsStringAsync(cancellationToken);

        return Guid.Parse(id.Trim('"'));
    }

    private HttpClient GetConfiguredHttpClient()
    {
        HttpClient? httpClient = null;
        try
        {
            httpClient = httpClientFactory();
            httpClient.Timeout = Timeout.InfiniteTimeSpan;
            return httpClient;
        }
        catch
        {
            httpClient?.Dispose();
            throw;
        }
    }
}