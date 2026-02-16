namespace SparkTrack.AvaloniaImpl.Services.Explorer;

public interface IExplorerService
{
    void OpenFolder(string path);

    void OpenContainingFolder(string path);
}