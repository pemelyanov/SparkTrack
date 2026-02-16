using System.Diagnostics;

namespace SparkTrack.AvaloniaImpl.Services.Explorer;

public class WindowsExplorerService : IExplorerService
{
    public void OpenFolder(string path)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true
            }
        );
    }

    public void OpenContainingFolder(string path)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{path}\"",
                UseShellExecute = true
            }
        );
    }
}