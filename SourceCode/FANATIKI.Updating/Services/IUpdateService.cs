namespace Fanatiki.Updating.Services;

using Loading.Data;

public interface IUpdateService
{
    #region Methods

    Task<Version?> TryGetNewerVersionAsync();
    
    Task<bool> TryUpdateAsync(IObserver<LoadingProgress>? observer = null);

    #endregion
}