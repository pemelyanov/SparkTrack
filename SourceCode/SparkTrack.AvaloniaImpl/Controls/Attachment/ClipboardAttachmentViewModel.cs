namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using System.Reactive.Linq;
using System.Windows.Input;
using Core.Client.Data;
using Core.Client.Services.Files;
using Core.Shared.Data.Entities;
using Extensions;
using NLog;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;

public class ClipboardAttachmentViewModel : AttachmentViewModelBase, IAttachmentViewModel, IUploadableAttachment
{
    private readonly IFilesService m_filesService;
    private const    string        Base64Mask = "base64:";

    public ClipboardAttachmentViewModel(
        string extension,
        byte[] data,
        Action<IAttachmentViewModel> onRemove,
        IDialogService dialogService,
        IFilesService filesService
    ) : base(onRemove, dialogService, LogManager.GetCurrentClassLogger())
    {
        m_filesService = filesService;
        Extension = extension;
        Size = data.LongLength;
        Name = Guid.NewGuid().ToString().Substring(0, 6) + "." + extension;
        Uri = Base64Mask + Convert.ToBase64String(data);
        IsImage = data.GetImageExtensionBySignature() is not null;
        CanOpenInExplorer = false;
    }

    protected override IAttachmentViewModel GetThis() => this;

    public bool IsDownloaded => true;

    public string Extension { get; }

    public long Size { get; }

    public Guid? UploadedFileId { get; private set; }

    [Reactive]
    public AttachmentLoadProgress? LoadProgress { get; private set; }

    public ICommand SaveAsCommand { get; } = ReactiveCommand.Create(() => { }, Observable.Return(false));

    public Task DownloadAsync() => throw new NotImplementedException();

    public override async Task RemoveAsync()
    {
        if (LoadProgress == null)
        {
            await base.RemoveAsync();
            Cancel(false);
        }

        Cancel();
    }

    public void Cancel()
    {
        Cancel(true);
    }

    public async Task UploadAsync()
    {
        m_cancellationTokenSource = new CancellationTokenSource();

        var progress = new AttachmentLoadProgress(ELoadType.Upload, new LoadingProgress());

        LoadProgress = progress;

        try
        {
            var data = GetBytes();
            UploadedFileId = await m_filesService.UploadAsync(data, progress.Progress, m_cancellationTokenSource.Token);
            
            var newPath = Path.Combine(
                s_downloadsFolder,
                $"{UploadedFileId.ToString()}.{Extension}"
            );
            
            File.WriteAllBytes(newPath, data);
        }
        catch (TaskCanceledException)
        {
            m_logger.Warn("File upload canceled");
        }
        finally
        {
            LoadProgress = null;
        }
    }

    public Attachment ToModel() => new()
    {
        Name = Name,
        Extension = Extension,
        Size = Size,
        FileId = UploadedFileId ?? throw new InvalidOperationException("Upload file before converting"),
        IsImage = IsImage,
        Checksum = Md5Helper.ComputeFileMd5(GetBytes())
    };

    private byte[] GetBytes()
    {
        var base64 = Uri.Substring(Base64Mask.Length);
        return Convert.FromBase64String(base64);
    }
}