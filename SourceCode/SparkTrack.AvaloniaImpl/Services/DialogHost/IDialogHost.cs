namespace SparkTrack.AvaloniaImpl.Services.DialogHost;

using ReactiveUI;

public interface IDialogHost
{
    Task<bool?> ShowAsync(ReactiveObject viewModel);
}