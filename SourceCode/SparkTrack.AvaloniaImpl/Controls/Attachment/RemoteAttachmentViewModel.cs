using SparkTrack.AvaloniaImpl.Services.Explorer;

namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Core.Client.Data;
using Core.Client.Services.Files;
using Core.Shared.Data.Entities;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using Services.LocalFilesManager;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using NLog;
using Services.AttachmentsPathCache;

public class RemoteAttachmentViewModel : AttachmentViewModelBase, IAttachmentViewModel
{
    private readonly Attachment    m_attachment;
    private readonly IFilesService m_filesService;

    public RemoteAttachmentViewModel(
        Attachment attachment,
        Action<IAttachmentViewModel> onRemove,
        IDialogService dialogService,
        ILocalFilesManager localFilesManager,
        IFilesService filesService,
        IAttachmentsPathCache attachmentsPathCache,
        IExplorerService explorerService
    )
        : base(onRemove, dialogService, explorerService, LogManager.GetCurrentClassLogger())
    {
        m_attachment = attachment;
        m_filesService = filesService;
        Name = attachment.Name;
        Extension = attachment.Extension;
        Size = attachment.Size;

        var cachedPath = attachmentsPathCache.Resolve(attachment.FileId);

        Uri = File.Exists(cachedPath) ? cachedPath : null ?? Path.Combine(
            s_downloadsFolder,
            $"{attachment.FileId.ToString()}.{attachment.Extension.TrimStart('.')}"
        );
        
        IsDownloaded = CheckIsDownloaded();

        if (IsDownloaded)
        {
            IsImage = CheckIsImage();
        }

        SaveAsCommand = ReactiveCommand.CreateFromTask(
            async () =>
            {
                if (!IsDownloaded)
                    await DownloadAsync();

                var targetPath = await localFilesManager.ChooseFileForSaveAsync(null, null, Extension);

                if (string.IsNullOrEmpty(targetPath)) return;

                try
                {
                    File.Copy(Uri, targetPath, true);
                }
                catch
                {
                    // TODO: возможно стоит добавить модалку
                    // ignore
                }
            }
        );
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        if (m_attachment.IsImage && !IsDownloaded) DownloadAsync().ToObservable().Subscribe().DisposeWith(disposables);
    }

    [Reactive]
    public bool IsDownloaded { get; private set; }

    public string Extension { get; }

    public long Size { get; }

    [Reactive]
    public AttachmentLoadProgress? LoadProgress { get; private set; }

    public ICommand SaveAsCommand { get; }
    
    public async Task DownloadAsync()
    {
        if (IsDownloaded || LoadProgress is not null) return;
        
        m_cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var progress = new AttachmentLoadProgress(ELoadType.Download, new LoadingProgress());

            LoadProgress = progress;

            await m_filesService.DownloadAsync(
                m_attachment.FileId,
                Uri,
                progress.Progress,
                m_cancellationTokenSource.Token
            );

            IsDownloaded = true;

            IsImage = CheckIsImage();
            Uri = new string(Uri); // Чтобы стригеррить обновление View
        }
        catch (TaskCanceledException)
        {
            m_logger.Warn("Download is cancelled");
        }
        catch
        {
            IsDownloaded = CheckIsDownloaded();
        }
        finally
        {
            LoadProgress = null;
        }
    }

    public override async Task RemoveAsync()
    {
        await base.RemoveAsync();
        Cancel();
    }

    public void Cancel()
    {
        Cancel(false);
    }

    public Attachment ToModel() => m_attachment;

    protected override IAttachmentViewModel GetThis() => this;
    
    private bool CheckIsDownloaded()
    {
        if (!File.Exists(Uri)) return false;

        return Md5Helper.VerifyFileMd5(Uri, m_attachment.Checksum);
    }
}