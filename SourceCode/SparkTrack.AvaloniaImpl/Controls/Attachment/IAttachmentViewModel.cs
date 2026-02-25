namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Core.Shared.Data.Entities;
using System.Windows.Input;

public interface IAttachmentViewModel
{
    event Action<IAttachmentViewModel> PreviewSetRequested;
    
    bool IsImage { get; }
    
    bool IsDownloaded { get; }
    
    string Uri { get; }
    
    string Name { get; }
    
    string Extension { get; }
    
    long Size { get; }

    AttachmentLoadProgress? LoadProgress { get; }
    
    long AverageSpeedBytesPerSecond { get; }
    
    TimeSpan? EstimatedTimeLeft { get; }

    bool CanOpenInExplorer { get; }

    ICommand SaveAsCommand { get; }
    
    ICommand GetLinkCommand { get; }

    Task RemoveAsync();

    Task DownloadAsync();

    void Cancel();

    void Open();

    void OpenInExplorer();

    Attachment ToModel();

    void RaisePreviewSetRequested();
}