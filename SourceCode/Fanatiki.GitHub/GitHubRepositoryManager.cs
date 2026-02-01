namespace Fanatiki.GitHub;

using System.Net.Http.Headers;
using BigHelp.Http;
using NLog;
using Octokit;
using ProductHeaderValue = Octokit.ProductHeaderValue;

public class GitHubRepositoryManager(string repoOwner, string repoName, string? accessToken = null)
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    
    public async Task<Release?> GetLatestReleaseAsync()
    {
        try
        {
            var client = new GitHubClient(new ProductHeaderValue(repoName));
            if(accessToken is not null) 
                client.Credentials = new Credentials(accessToken);
            Release? release = await client.Repository.Release.GetLatest(repoOwner, repoName);

            return release;
        }
        catch
        {
            return null;
        }
    }
    
    public async Task DownloadAsset(
        ReleaseAsset asset,
        string downloadPath,
        IProgress<HttpDownloadProgress>? progressTracker = null
    )
    {
        var downloadUrl = asset.BrowserDownloadUrl;
        s_logger.Info("Downloading asset: {url} -> {path}", downloadUrl, downloadPath);

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)"
        );
        
        if(accessToken is not null)
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        httpClient.DefaultRequestHeaders.Add("Accept", "application/octet-stream");

        await httpClient.DownloadFileAsync(
            downloadUrl,
            Path.GetDirectoryName(downloadPath)!,
            Path.GetFileName(downloadPath),
            progressTracker
        );
    }
}