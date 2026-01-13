namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Extensions;
using Fanatiki.MVVM.ViewModels;
using ImageDialog;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using System.Diagnostics;

public abstract class AttachmentViewModelBase(
    Action<IAttachmentViewModel> onRemove,
    IDialogHost dialogHost
) : ViewModelBase
{
    [Reactive]
    public bool IsImage { get; protected set; }

    public string Uri { get; protected set; } = string.Empty;

    public string Name { get; protected set; } = string.Empty;
    
    public void Remove()
    {
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

        dialogHost.ShowAsync(imageViewModel);
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
}