namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Core.Client.Data;
using Core.Client.Services.Files;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using ImageDialog;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;

public class LocalAttachmentViewModel : ViewModelBase, IAttachmentViewModel
{
    private readonly Action<IAttachmentViewModel> m_onRemove;
    private readonly IDialogHost                  m_dialogHost;
    private readonly IFilesService                m_filesService;

    public LocalAttachmentViewModel(
        string path,
        Action<IAttachmentViewModel> onRemove,
        IDialogHost dialogHost,
        IFilesService filesService
    )
    {
        m_onRemove = onRemove;
        m_dialogHost = dialogHost;
        m_filesService = filesService;
        using var stream = File.OpenRead(path);

        IsImage = stream.IsImageBySignature();
        Uri = path;
        Name = Path.GetFileName(path);
        Extension = Path.GetExtension(path).TrimStart('.');
        Size = stream.Length;
    }

    public bool IsImage { get; }

    public bool IsDownloaded => true;

    public string Uri { get; }

    public string Name { get; }

    public string Extension { get; }

    public long Size { get; }

    public Guid? UploadedFileId { get; private set; }

    [Reactive]
    public AttachmentLoadProgress? LoadProgress { get; private set; }

    public ICommand SaveAsCommand { get; } = ReactiveCommand.Create(() => { }, Observable.Return(false));

    public void Remove()
    {
        m_onRemove.Invoke(this);
    }

    public Task DownloadAsync() => throw new NotImplementedException();

    public async Task UploadAsync()
    {
        var progress = new AttachmentLoadProgress(ELoadType.Upload, new LoadingProgress());

        LoadProgress = progress;

        UploadedFileId = await m_filesService.UploadAsync(Uri, progress.Progress);

        LoadProgress = null;
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

        m_dialogHost.ShowAsync(imageViewModel);
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