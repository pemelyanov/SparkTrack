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
using System.Windows.Input;
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

    private          IDisposable?  m_progressSubscription;
    private          DateTime?     m_lastUpdateTime;
    private          long          m_lastBytesTransferred;
    private readonly Queue<double> m_speedSamples  = new(); // Для сглаживания скорости
    private const    int           MaxSpeedSamples = 64; // Количество семплов для усреднения

    public event Action<IAttachmentViewModel>? PreviewSetRequested;

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        this.WhenAnyValue(it => it.LoadProgress)
            .Skip(1)
            .DistinctUntilChanged()
            .Subscribe(progress =>
                {
                    m_progressSubscription?.Dispose();
                    m_progressSubscription = null;

                    if (progress is null)
                    {
                        m_logger.Info("Progress removed");
                        AverageSpeedBytesPerSecond = 0;
                        EstimatedTimeLeft = null;
                        m_speedSamples.Clear();
                        m_lastUpdateTime = null;
                        m_lastBytesTransferred = 0;
                        return;
                    }
                    
                    var progressSubscription = progress.Progress.CurrentProgress.CombineLatest(
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
                    
                    var speedSubscription =
                        Observable.Interval(TimeSpan.FromSeconds(1))
                            .StartWith(0)
                            .Subscribe(_ => UpdateSpeedAndTime(progress));

                    m_progressSubscription = new CompositeDisposable(progressSubscription, 
                        speedSubscription);
                }
            )
            .DisposeWith(disposables);

        Disposable.Create(() =>
            {
                var subscription = m_progressSubscription;
                m_progressSubscription = null;
                subscription?.Dispose();
            }
        ).DisposeWith(disposables);
    }

    [Reactive]
    public AttachmentLoadProgress? LoadProgress { get; protected set; }

    [Reactive]
    public long AverageSpeedBytesPerSecond { get; private set; }

    [Reactive]
    public TimeSpan? EstimatedTimeLeft { get; private set; }

    [Reactive]
    public bool IsImage { get; protected set; }

    [Reactive]
    public string Uri { get; protected set; } = string.Empty;

    public string Name { get; protected set; } = string.Empty;

    public bool CanOpenInExplorer { get; protected set; } = true;

    public ICommand GetLinkCommand { get; init; } = ReactiveCommand.Create(() => { }, Observable.Return(false));

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

            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = Uri,
                        UseShellExecute = true
                    }
                );
            }
            catch (Exception e)
            {
                m_logger.Warn(e, $"Error while starting procces for {Name}");
            }
            
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

    public void RaisePreviewSetRequested() => PreviewSetRequested?.Invoke(GetThis());

    protected void Cancel(bool close)
    {
        m_logger.Info("Canceling file upload (base call)");
        m_cancellationTokenSource?.Cancel();
        m_cancellationTokenSource = null;

        if (close)
            onRemove(GetThis());
    }

    private void UpdateSpeedAndTime(AttachmentLoadProgress progress)
    {
        try
        {
            var currentBytes = progress.Progress.CurrentProgress.Value;
            var totalBytes = progress.Progress.TotalProgress.Value;
            var now = DateTime.UtcNow;

            if (m_lastUpdateTime.HasValue && m_lastBytesTransferred > 0)
            {
                var timeDiff = (now - m_lastUpdateTime.Value).TotalSeconds;
                if (timeDiff > 0)
                {
                    var bytesDiff = currentBytes - m_lastBytesTransferred;
                    var instantSpeed = bytesDiff / timeDiff;
                    
                    if (instantSpeed >= 0) 
                    {
                        m_speedSamples.Enqueue(instantSpeed);
                        if (m_speedSamples.Count > MaxSpeedSamples)
                        {
                            m_speedSamples.Dequeue();
                        }
                        
                        AverageSpeedBytesPerSecond = (long)m_speedSamples.Average();
                        
                        if (AverageSpeedBytesPerSecond > 0 && totalBytes > 0)
                        {
                            var remainingBytes = totalBytes - currentBytes;
                            if (remainingBytes > 0)
                            {
                                var remainingSeconds = remainingBytes / AverageSpeedBytesPerSecond;
                                EstimatedTimeLeft = TimeSpan.FromSeconds(remainingSeconds);
                            }
                            else
                            {
                                EstimatedTimeLeft = TimeSpan.Zero;
                            }
                        }
                        else
                        {
                            EstimatedTimeLeft = null;
                        }

                        m_logger.Trace(
                            "Speed update: {Speed:F2} KB/s, remaining: {Remaining}, samples: {SamplesCount}",
                            AverageSpeedBytesPerSecond / 1024,
                            EstimatedTimeLeft?.ToString(@"hh\:mm\:ss") ?? "unknown",
                            m_speedSamples.Count
                        );
                    }
                }
            }

            m_lastUpdateTime = now;
            m_lastBytesTransferred = currentBytes;
        }
        catch (Exception ex)
        {
            m_logger.Error(ex, "Error calculating speed for attachment {Name}", Name);
        }
    }
}