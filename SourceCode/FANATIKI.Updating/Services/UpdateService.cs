namespace Fanatiki.Updating.Services;

using System.Reflection;
using Loading.Data;
using NLog;

internal class UpdateService(
    string applicationRootPath,
    IUpdateLoaderService loaderService,
    IUpdateUnpackerService unpackerService
)
    : IUpdateService
{
    #region Fields

    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Methods

    public async Task<Version?> TryGetNewerVersionAsync()
    {
        s_logger.Info("Fetching for new versions...");
        Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version!;
        Version? latestVersion = await loaderService.GetLatestVersionAsync();

        s_logger.Info("Fetched version: {version}", latestVersion);
        s_logger.Info("Current version: {version}", currentVersion);

        if (latestVersion is null || latestVersion <= currentVersion)
        {
            s_logger.Info("Fetched version not greater than current, skipping update");
            
            return null;
        }

        return latestVersion;
    }

    public async Task<bool> TryUpdateAsync(IObserver<LoadingProgress>? observer)
    {
        s_logger.Info("Fetching for new versions...");
        observer?.OnNext(
            new LoadingProgress
            {
                StageName = "Проверяем обновления на сервере...",
            }
        );
        Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version!;
        Version? latestVersion = await loaderService.GetLatestVersionAsync();

        s_logger.Info("Fetched version: {version}", latestVersion);
        s_logger.Info("Current version: {version}", currentVersion);

        if (latestVersion is null || latestVersion <= currentVersion)
        {
            s_logger.Info("Fetched version not greater than current, skipping update");
            return false;
        }

        s_logger.Info("Downloading version...");
        observer?.OnNext(
            new LoadingProgress
            {
                StageName = "Начинаем загрузку обновления...",
            }
        );
        string? updatePath = await loaderService.DownloadLatestAsync(observer);

        if (updatePath is null)
        {
            s_logger.Warn("Vesrion not downloaded");
            return false;
        }

        s_logger.Info("Version downloaded by path: {path}", updatePath);
        s_logger.Info("Current executable folder: {folder}", applicationRootPath);

        observer?.OnNext(
            new LoadingProgress
            {
                StageName = "Выполняем миграции..."
            }
        );

        await DoMigrations(currentVersion, latestVersion);
        
        observer?.OnNext(
            new LoadingProgress
            {
                StageName = "Начинаем распаковку..."
            }
        );

        return unpackerService.BeginUnpack(applicationRootPath, updatePath);
    }

    private Task DoMigrations(Version currentVersion, Version previousVersion)
    {
        s_logger.Info("Initializing migrations from {prev} to {current}...", previousVersion, currentVersion);

        // TODO: Add Migrations when needed
        
        s_logger.Info("Migrations completed");
        return Task.CompletedTask;
    }
    #endregion
}