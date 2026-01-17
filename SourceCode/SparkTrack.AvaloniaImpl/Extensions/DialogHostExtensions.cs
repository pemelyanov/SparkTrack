namespace SparkTrack.AvaloniaImpl.Extensions;

using Services.DialogHost;
using Windows.Confirmation;
using Windows.Notification;

public static class DialogHostExtensions
{
    public static Task NotifyAsync(
        this IDialogHost host,
        string message,
        string? title = null,
        string? buttonText = null
    ) => host.ShowAsync(new NotificationViewModel(message, title, buttonText));

    public static async Task<bool> ConfirmAsync(
        this IDialogHost host,
        string message,
        string? title = null,
        string? acceptText = null,
        string? cancelText = null
    ) => await host.ShowAsync(new ConfirmationViewModel(message, title, acceptText, cancelText)) ?? false;
}