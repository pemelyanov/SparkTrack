namespace SparkTrack.AvaloniaImpl.ViewModels;

using Fanatiki.MVVM.ViewModels;

public class DialogViewModelBase : ViewModelBase
{
    public bool IsClosed { get; private set; }
    
    public bool? Result { get; private set; }
    
    public event Action<bool?>? CloseSignal;

    public void Close(bool? result)
    {
        Result = result;
        IsClosed = true;
        CloseSignal?.Invoke(result);
    }
}