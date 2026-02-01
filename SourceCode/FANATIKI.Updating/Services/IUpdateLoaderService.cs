namespace Fanatiki.Updating.Services;

using Loading.Data;

public interface IUpdateLoaderService
{
    #region Methods

    Task<Version?> GetLatestVersionAsync();

    Task<string?> DownloadLatestAsync(IObserver<LoadingProgress>? observer = null);

    #endregion
}