namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Core.Client.Data;
using Core.Client.Services.Files;
using Core.Shared.Data.Entities;
using Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using System.Reactive.Linq;
using System.Windows.Input;

public class LocalAttachmentViewModel : AttachmentViewModelBase, IAttachmentViewModel
{
    private readonly IFilesService m_filesService;

    public LocalAttachmentViewModel(
        string path,
        Action<IAttachmentViewModel> onRemove,
        IDialogService dialogService,
        IFilesService filesService
    ) : base(onRemove, dialogService)
    {
        m_filesService = filesService;
        using var stream = File.OpenRead(path);

        IsImage = stream.IsImageBySignature();
        Uri = path;
        Name = Path.GetFileName(path);
        Extension = Path.GetExtension(path).TrimStart('.');
        Size = stream.Length;
    }

    public bool IsDownloaded => true;

    public string Extension { get; }

    public long Size { get; }

    public Guid? UploadedFileId { get; private set; }

    [Reactive]
    public AttachmentLoadProgress? LoadProgress { get; private set; }

    public ICommand SaveAsCommand { get; } = ReactiveCommand.Create(() => { }, Observable.Return(false));

    public Task DownloadAsync() => throw new NotImplementedException();

    public async Task UploadAsync()
    {
        var progress = new AttachmentLoadProgress(ELoadType.Upload, new LoadingProgress());

        LoadProgress = progress;

        UploadedFileId = await m_filesService.UploadAsync(Uri, progress.Progress);

        LoadProgress = null;
    }

    public Attachment ToModel() => new()
    {
        Name = Name,
        Extension = Extension,
        Size = Size,
        FileId = UploadedFileId ?? throw new InvalidOperationException("Upload file before converting"),
        IsImage = IsImage,
        Checksum = Md5Helper.ComputeFileMd5(Uri)
    };

    protected override IAttachmentViewModel GetThis() => this;
}