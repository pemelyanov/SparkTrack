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
using Windows.LinkShare;
using Core.Client.Enums;
using Core.Client.Services.PopupNotification;
using NLog;
using Services.AttachmentsPathCache;

public class RemoteAttachmentViewModel : AttachmentViewModelBase, IAttachmentViewModel
{
    private readonly Attachment                m_attachment;
    private readonly IFilesService             m_filesService;
    private readonly IPopupNotificationService m_popupNotificationService;

    public RemoteAttachmentViewModel(
        Attachment attachment,
        Action<IAttachmentViewModel> onRemove,
        IDialogService dialogService,
        ILocalFilesManager localFilesManager,
        IFilesService filesService,
        IAttachmentsPathCache attachmentsPathCache,
        IExplorerService explorerService,
        IPopupNotificationService popupNotificationService,
        Func<Func<Task<string>>, LinkShareViewModel> linkShareFactory
    )
        : base(onRemove, dialogService, explorerService, LogManager.GetCurrentClassLogger())
    {
        m_attachment = attachment;
        m_filesService = filesService;
        m_popupNotificationService = popupNotificationService;
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
            IsImage = CheckIsImage(Uri);
        }

        SaveAsCommand = ReactiveCommand.CreateFromTask(
            async () => { await SaveAsAsync(localFilesManager); }
        );

        GetLinkCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var shareViewModel = linkShareFactory(() => filesService.GetLinkAsync(attachment.FileId));
                await dialogService.ShowAsync(shareViewModel);
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

    public ICommand SaveAsCommand { get; }
    
    public async Task DownloadAsync()
    {
        if (IsDownloaded || LoadProgress is not null) return;
        
        m_cancellationTokenSource = new CancellationTokenSource();
        
        var uri = Uri;
        
        try
        {
            Uri = string.Empty;
            var progress = new AttachmentLoadProgress(ELoadType.Download, new LoadingProgress());

            LoadProgress = progress;

            await m_filesService.DownloadAsync(
                m_attachment.FileId,
                uri,
                progress.Progress,
                m_cancellationTokenSource.Token
            );

            IsDownloaded = true;

            IsImage = CheckIsImage(uri);
        }
        catch (TaskCanceledException)
        {
            m_logger.Warn("Download is cancelled. Removing file...");
            if (File.Exists(uri)) File.Delete(uri);
        }
        catch (Exception e)
        {
            m_logger.Error(e, "Download failed");
            m_popupNotificationService.Show(
                ENotificationType.Error,
                $"При загрузке файла {Name}.{Extension} произошла ошибка"
            );
            IsDownloaded = CheckIsDownloaded();
        }
        finally
        {
            Uri = uri;  // Чтобы стригеррить обновление View
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

        var fileInfo = new FileInfo(Uri);

        return fileInfo.Length == Size;
        // MD5 Занимает оч много времени, пока проверяем просто по размеру
        return Md5Helper.VerifyFileMd5(Uri, m_attachment.Checksum);
    }
    
    private async Task SaveAsAsync(ILocalFilesManager localFilesManager)
    {
        if (!IsDownloaded)
            await DownloadAsync();

        var targetPath = await localFilesManager.ChooseFileForSaveAsync(null, null, Extension);

        if (string.IsNullOrEmpty(targetPath)) return;

        try
        {
            File.Copy(Uri, targetPath, true);
        }
        catch (Exception e)
        {
            // TODO: возможно стоит добавить модалку
            m_logger.Warn(e, "Move to {targetPath} failed.", targetPath);
        }
    }
}