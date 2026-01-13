namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Core.Shared.Data.Entities;
using System.Windows.Input;

public interface IAttachmentViewModel
{
    bool IsImage { get; }
    
    bool IsDownloaded { get; }
    
    string Uri { get; }
    
    string Name { get; }
    
    string Extension { get; }
    
    long Size { get; }

    AttachmentLoadProgress? LoadProgress { get; }
    
    ICommand SaveAsCommand { get; }

    void Remove();

    Task DownloadAsync();

    void Open();

    void OpenInExplorer();

    Attachment ToModel();
}