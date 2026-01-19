namespace SparkTrack.AvaloniaImpl.Services.DialogHost;

using ReactiveUI;

public interface IDialogService
{
    Task<bool?> ShowAsync(ReactiveObject viewModel);
}