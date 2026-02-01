namespace Fanatiki.Updating.Services;

public interface IUpdateUnpackerService
{
    #region Methods

    bool BeginUnpack(string launcherUnpackPath, string updatePath);

    #endregion
}