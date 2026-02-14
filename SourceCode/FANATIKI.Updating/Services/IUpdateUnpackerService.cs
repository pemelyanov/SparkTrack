namespace Fanatiki.Updating.Services;

public interface IUpdateUnpackerService
{
    #region Methods

    bool BeginUnpack(string applicationRootPath, string updatePath);

    #endregion
}