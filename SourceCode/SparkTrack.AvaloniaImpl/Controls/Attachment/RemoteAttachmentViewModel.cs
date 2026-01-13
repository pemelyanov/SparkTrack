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

public class RemoteAttachmentViewModel : AttachmentViewModelBase, IAttachmentViewModel
{
    private readonly Attachment    m_attachment;
    private readonly IFilesService m_filesService;

    private static readonly string s_downloadsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Downloads",
        "SparkTrackDownloads"
    );

    public RemoteAttachmentViewModel(
        Attachment attachment,
        Action<IAttachmentViewModel> onRemove,
        IDialogHost dialogHost,
        ILocalFilesManager localFilesManager,
        IFilesService filesService
    )
        : base(onRemove, dialogHost)
    {
        m_attachment = attachment;
        m_filesService = filesService;
        Name = attachment.Name;
        Extension = attachment.Extension;
        Size = attachment.Size;

        Uri = Path.Combine(
            s_downloadsFolder,
            $"{attachment.FileId.ToString()}.{attachment.Extension.TrimStart('.')}"
        );

        // TODO: Добавить проверку MD5 суммы файла
        IsDownloaded = File.Exists(Uri);

        if (IsDownloaded)
        {
            IsImage = CheckIsImage();
        }

        SaveAsCommand = ReactiveCommand.CreateFromTask(
            async () =>
            {
                if (!IsDownloaded)
                    await DownloadAsync();

                var targetPath = await localFilesManager.ChooseFileForSaveAsync();

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

        if (IsImage && !IsDownloaded) DownloadAsync().ToObservable().Subscribe().DisposeWith(disposables);
    }

    [Reactive]
    public bool IsDownloaded { get; private set; }

    public string Extension { get; }

    public long Size { get; }

    public AttachmentLoadProgress? LoadProgress { get; private set; }

    public ICommand SaveAsCommand { get; }

    public async Task DownloadAsync()
    {
        if (IsDownloaded) return;

        IsDownloaded = true;

        try
        {
            var progress = new AttachmentLoadProgress(ELoadType.Download, new LoadingProgress());

            LoadProgress = progress;

            await m_filesService.DownloadAsync(m_attachment.FileId, Uri, progress.Progress);

            IsImage = CheckIsImage();

            LoadProgress = null;
        }
        catch
        {
            IsDownloaded = File.Exists(Uri);
        }
    }

    public Attachment ToModel() => m_attachment;

    protected override IAttachmentViewModel GetThis() => this;
}