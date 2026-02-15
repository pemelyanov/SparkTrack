namespace SparkTrack.AvaloniaImpl.Extensions;

using Services.DialogHost;
using Windows.Confirmation;
using Windows.Notification;
using ViewModels;

public static class DialogHostExtensions
{
    public static Task NotifyAsync(
        this IDialogService service,
        string message,
        string? title = null,
        string? buttonText = null
    ) => service.ShowAsync(new NotificationViewModel(message, title, buttonText));

    public static async Task<bool> ConfirmAsync(
        this IDialogService service,
        string message,
        string? title = null,
        string? acceptText = null,
        string? cancelText = null,
        IReadOnlyList<SelectableViewModel<string>>? additionalOptionsList = null
    ) => await service.ShowAsync(
        new ConfirmationViewModel(message, title, acceptText, cancelText, additionalOptionsList)
    ) ?? false;
}