using SparkTrack.AvaloniaImpl.Services.Explorer;

namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Core.Client.Data;
using Core.Client.Services.Files;
using Core.Shared.Data.Entities;
using Extensions;
using ReactiveUI;
using Services.DialogHost;
using System.Reactive.Linq;
using System.Windows.Input;
using Exceptions;
using NLog;
using Services.AttachmentsPathCache;

public class LocalAttachmentViewModel : AttachmentViewModelBase, IAttachmentViewModel, IUploadableAttachment
{
    private readonly IFilesService         m_filesService;
    private readonly IAttachmentsPathCache m_attachmentsPathCache;

    public LocalAttachmentViewModel(
        string path,
        Action<IAttachmentViewModel> onRemove,
        IDialogService dialogService,
        IFilesService filesService,
        IAttachmentsPathCache attachmentsPathCache,
        IExplorerService explorerService
    ) : base(onRemove, dialogService, explorerService, LogManager.GetCurrentClassLogger())
    {
        m_filesService = filesService;
        m_attachmentsPathCache = attachmentsPathCache;
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
            UploadedFileId = await m_filesService.UploadAsync(Uri, progress.Progress, m_cancellationTokenSource.Token);
            m_attachmentsPathCache.Save(UploadedFileId.Value, Uri);
        }
        catch (TaskCanceledException)
        {
            m_logger.Warn("File upload canceled");
        }
        catch (Exception e)
        {
            throw new NotifyUIException($"При отправке файла {Name}.{Extension} произошла ошибка", e);
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
        Checksum = Md5Helper.ComputeFileMd5(Uri)
    };

    protected override IAttachmentViewModel GetThis() => this;
}