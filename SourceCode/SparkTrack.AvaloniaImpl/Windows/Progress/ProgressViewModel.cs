namespace SparkTrack.AvaloniaImpl.Windows.Progress;

using System.Reactive.Subjects;
using System.Windows.Input;
using ReactiveUI;
using ViewModels;

public class ProgressViewModel : DialogViewModelBase
{
    private readonly CancellationTokenSource? m_cancellationTokenSource;
    private readonly BehaviorSubject<bool>    m_canCancel = new(false);

    public ProgressViewModel(
        string message,
        string? title = null,
        CancellationTokenSource? cancellationTokenSource = null
    ) : this(message, title, cancellationTokenSource?.Token ?? CancellationToken.None)
    {
        m_cancellationTokenSource = cancellationTokenSource;
        m_canCancel.OnNext(true);
    }

    public ProgressViewModel(string message, string? title = null, CancellationToken cancellationToken = default)
    {
        Message = message;
        Title = title ?? "Загрузка";

        cancellationToken.Register(() => Close(null));
        CancelCommand = ReactiveCommand.Create(Cancel, m_canCancel);
    }

    public string Message { get; }

    public string Title { get; }
    
    public ICommand CancelCommand { get; }

    private void Cancel()
    {
        if (m_cancellationTokenSource is null)
        {
            Close(null);
            return;
        }
        
        m_canCancel.OnNext(false);
        m_cancellationTokenSource.Cancel();
    }
}