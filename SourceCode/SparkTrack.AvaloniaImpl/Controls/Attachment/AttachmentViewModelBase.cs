namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Extensions;
using Fanatiki.MVVM.ViewModels;
using ImageDialog;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using System.Diagnostics;
using NLog;

public abstract class AttachmentViewModelBase(
    Action<IAttachmentViewModel> onRemove,
    IDialogService dialogService,
    ILogger logger
) : ViewModelBase
{
    protected readonly ILogger                  m_logger = logger;
    protected          CancellationTokenSource? m_cancellationTokenSource;
    
    [Reactive]
    public bool IsImage { get; protected set; }

    public string Uri { get; protected set; } = string.Empty;

    public string Name { get; protected set; } = string.Empty;

    public bool CanOpenInExplorer { get; protected set; } = true; 
    
    public virtual async Task RemoveAsync()
    {
        if (!await dialogService.ConfirmAsync(
            "Вы действительно хотите удалить файл?",
            "Удаление файла"
        )) return;
        
        onRemove.Invoke(GetThis());
    }

    protected abstract IAttachmentViewModel GetThis();
    
    protected bool CheckIsImage()
    {
        using var fileStream = File.OpenRead(Uri);
        var isImage = fileStream.IsImageBySignature();
        return isImage;
    }
    
    public void Open()
    {
        if (!IsImage)
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = Uri,
                    UseShellExecute = true
                }
            );

            return;
        }

        var imageViewModel = new ImageDialogViewModel(Name, Uri);

        dialogService.ShowAsync(imageViewModel);
    }

    public void OpenInExplorer()
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{Uri}\"",
                UseShellExecute = true
            }
        );
    }

    protected void Cancel(bool close)
    {
        m_logger.Info("Canceling file upload...");
        m_cancellationTokenSource?.Cancel();
        m_cancellationTokenSource = null;
        
        if(close)
            onRemove(GetThis());
    }
}