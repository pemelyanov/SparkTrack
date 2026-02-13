namespace Fanatiki.Updating.Services;

using System.Diagnostics;
using NLog;

public class InnoSetupInstallerUnpackerService
    : IUpdateUnpackerService
{
    #region Fields

    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Methods

    public bool BeginUnpack(string applicationRootPath, string updatePath)
    {
        if (!File.Exists(updatePath))
        {
            s_logger.Warn("Cannot find setup executable: {path}", updatePath);
            return false;
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = updatePath,
            Arguments = $"/DIR \"{applicationRootPath}\" /CLOSEAPPLICATIONS /SILENT /NOCANCEL /AUTORUN=1 /AUTOREMOVE=1",
        };

        s_logger.Info("Starting setup: {path} {args}", updatePath, processStartInfo.Arguments);

        Process.Start(processStartInfo);

        return true;
    }

    #endregion
}