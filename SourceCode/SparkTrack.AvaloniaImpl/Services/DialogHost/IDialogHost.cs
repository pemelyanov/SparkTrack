namespace SparkTrack.AvaloniaImpl.Services.DialogHost;

using ReactiveUI;

public interface IDialogHost
{
    Task ShowAsync(ReactiveObject viewModel);
}