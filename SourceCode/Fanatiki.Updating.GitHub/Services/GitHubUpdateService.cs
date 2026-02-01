namespace Fanatiki.Updating.GitHub.Services;

using System.Reactive.Subjects;
using BigHelp.Http;
using Fanatiki.GitHub;
using Loading.Data;
using NLog;
using Octokit;
using Updating.Services;

public class GitHubUpdateService(string repoName, string repoOwner, string accessToken) : IUpdateLoaderService
{
    #region Fields

    private readonly        GitHubRepositoryManager m_gitHubRepositoryManager = new(repoOwner, repoName, accessToken);
    private static readonly ILogger                 s_logger                  = LogManager.GetCurrentClassLogger();

    #endregion

    #region Methods

    public async Task<Version?> GetLatestVersionAsync()
    {
        Release? latestRelease = await m_gitHubRepositoryManager.GetLatestReleaseAsync();

        if (latestRelease is null) return null;

        return new Version(latestRelease.TagName.TrimStart('v'));
    }

    public async Task<string?> DownloadLatestAsync(IObserver<LoadingProgress>? observer)
    {
        s_logger.Info("Downloading latest release...");
        Release? latestRelease = await m_gitHubRepositoryManager.GetLatestReleaseAsync();

        if (latestRelease is null) return null;

        s_logger.Info("Release found: {asset}...", latestRelease.TagName);

        ReleaseAsset? launcherAsset =
            latestRelease.Assets.FirstOrDefault(it => it.Name.StartsWith(repoName) && it.Name.EndsWith(".zip"));

        if (launcherAsset is null) return null;

        s_logger.Info("Asset found: {asset}...", launcherAsset.BrowserDownloadUrl);

        string updatePath = Path.GetTempFileName();

        await DownloadUpdateAsync(launcherAsset, updatePath, observer);

        return updatePath;
    }

    private async Task DownloadUpdateAsync(
        ReleaseAsset releaseAsset,
        string downloadPath,
        IObserver<LoadingProgress>? observer
    )
    {
        s_logger.Info("Downloading asset: {url} -> {path}", releaseAsset.BrowserDownloadUrl, downloadPath);
        var stage = new LoadingProgress
        {
            StageName = "Загружаем обновление...",
            TotalTasksQuantity = new BehaviorSubject<int>(100),
        };
        observer?.OnNext(stage);

        await m_gitHubRepositoryManager.DownloadAsset(
            releaseAsset,
            downloadPath,
            new Progress<HttpDownloadProgress>(progress =>
                {
                    s_logger.Trace("Download progress: {percent}%", progress.PercentDownloaded * 100);
                    stage.ProcessedTasksQuantity.OnNext((int)(progress.PercentDownloaded * 100));
                }
            )
        );
    }

    #endregion
}