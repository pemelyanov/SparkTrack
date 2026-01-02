namespace SparkTrack.AvaloniaImpl.ViewModels;

using Fanatiki.MVVM.ViewModels;

public class DialogViewModelBase : ViewModelBase
{
    public event Action<bool?>? CloseSignal;

    public void Close(bool? result) => CloseSignal?.Invoke(result);
}