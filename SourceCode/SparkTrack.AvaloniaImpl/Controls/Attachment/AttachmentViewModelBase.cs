using SparkTrack.AvaloniaImpl.Services.Explorer;

namespace SparkTrack.AvaloniaImpl.Controls.Attachment;

using Extensions;
using Fanatiki.MVVM.ViewModels;
using ImageDialog;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using NLog;
using ReactiveUI;

public abstract class AttachmentViewModelBase(
    Action<IAttachmentViewModel> onRemove,
    IDialogService dialogService,
    IExplorerService explorerService,
    ILogger logger
) : ViewModelBase
{
    protected readonly ILogger                  m_logger = logger;
    protected          CancellationTokenSource? m_cancellationTokenSource;
    protected static readonly string s_downloadsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Downloads",
        "SparkTrackDownloads"
    );

    private IDisposable? m_progressSubscription;

    public event Action<IAttachmentViewModel>? PreviewSetRequested;

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        this.WhenAnyValue(it => it.LoadProgress)
            .Skip(1)
            .DistinctUntilChanged()
            .Subscribe(progress =>
                {
                    if (progress is null)
                    {
                        m_logger.Info("Progress removed");
                        m_progressSubscription?.Dispose();
                        m_progressSubscription = null;
                        return;
                    }

                    m_progressSubscription = progress.Progress.CurrentProgress.CombineLatest(
                            progress.Progress.TotalProgress,
                            (current, total) => new
                            {
                                current,
                                total
                            }
                        )
                        .Subscribe(args => m_logger.Trace(
                                "Progress for attachment '{name}' changed: {bytes}/{total} ({percent:P})",
                                Name,
                                args.current,
                                args.total,
                                (float)args.current / args.total
                            )
                        );
                }
            )
            .DisposeWith(disposables);
    }
    
    [Reactive]
    public AttachmentLoadProgress? LoadProgress { get; protected set; }

    [Reactive]
    public bool IsImage { get; protected set; }

    [Reactive]
    public string Uri { get; protected set; } = string.Empty;

    public string Name { get; protected set; } = string.Empty;

    public bool CanOpenInExplorer { get; protected set; } = true; 
    
    public virtual async Task RemoveAsync()
    {
        m_logger.Info("Attempt to remove attachment (base call)");
        
        if (!await dialogService.ConfirmAsync(
            "Вы действительно хотите удалить файл?",
            "Удаление файла"
        )) return;
        
        onRemove.Invoke(GetThis());
    }

    protected abstract IAttachmentViewModel GetThis();
    
    protected bool CheckIsImage(string uri)
    {
        using var fileStream = File.OpenRead(uri);
        var isImage = fileStream.IsImageBySignature();
        return isImage;
    }
    
    public void Open()
    {
        m_logger.Info("Attempt to open attachment");
        
        if (!IsImage)
        {
            m_logger.Info("Attachment is not image, starting process for {uri}", Uri);
            
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = Uri,
                    UseShellExecute = true
                }
            );

            return;
        }

        m_logger.Info("Attachment is image, openning preview dialog");
        var imageViewModel = new ImageDialogViewModel(Name, Uri);

        dialogService.ShowAsync(imageViewModel);
    }

    public void OpenInExplorer()
    {
        m_logger.Info("Attempt to open attachment in explorer");
        explorerService.OpenContainingFolder(Uri);
    }

    protected void Cancel(bool close)
    {
        m_logger.Info("Canceling file upload (base call)");
        m_cancellationTokenSource?.Cancel();
        m_cancellationTokenSource = null;
        
        if(close)
            onRemove(GetThis());
    }

    public void RaisePreviewSetRequested() => PreviewSetRequested?.Invoke(GetThis());
}