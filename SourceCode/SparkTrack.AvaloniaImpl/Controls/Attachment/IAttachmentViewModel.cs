namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

public interface IAttachmentViewModel
{
    bool IsImage { get; }
    
    bool IsDownloaded { get; }
    
    string Uri { get; }
    
    string Name { get; }
    
    long Size { get; }

    void Remove();

    Task DownloadAsync();

    void Open();

    void OpenInExplorer();
}