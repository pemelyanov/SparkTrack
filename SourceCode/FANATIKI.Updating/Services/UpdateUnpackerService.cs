namespace Fanatiki.Updating.Services;

using System.Diagnostics;
using NLog;

public class UpdateUnpackerService(string updatedUnpackerPath, string currentUnpackerPath)
    : IUpdateUnpackerService
{
    #region Fields

    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Methods

    public bool BeginUnpack(string launcherUnpackPath, string updatePath)
    {
        if (File.Exists(updatedUnpackerPath))
        {
            s_logger.Info("New unpacker founded, moving to {newFile}", currentUnpackerPath);
            var attempts = 5;
            while (attempts > 0)
                try
                {
                    File.Move(
                        updatedUnpackerPath,
                        currentUnpackerPath,
                        true
                    );

                    if (File.Exists(updatedUnpackerPath))
                        File.Delete(updatedUnpackerPath);

                    break;
                }
                catch
                {
                    s_logger.Warn("Move failed, retrying... Remaining attempts: {attempts}", attempts);
                    Thread.Sleep(500);
                    attempts--;
                }
        }

        if (!File.Exists(currentUnpackerPath))
        {
            s_logger.Warn("Cannot find current unpacker executable");
            return false;
        }

        string processName = Process.GetCurrentProcess().ProcessName;

        var processStartInfo = new ProcessStartInfo
        {
            FileName = currentUnpackerPath,
            Arguments = $"-p \"{processName}\" -l \"{launcherUnpackPath}\" -u \"{updatePath}\"",
        };

        s_logger.Info("Starting unpacker with args: {args}", processStartInfo.Arguments);

        Process.Start(processStartInfo);

        return true;
    }

    #endregion
}