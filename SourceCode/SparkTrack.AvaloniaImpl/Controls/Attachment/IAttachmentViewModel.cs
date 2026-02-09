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

    bool CanOpenInExplorer { get; }

    ICommand SaveAsCommand { get; }

    Task RemoveAsync();

    Task DownloadAsync();

    void Cancel();

    void Open();

    void OpenInExplorer();

    Attachment ToModel();
}