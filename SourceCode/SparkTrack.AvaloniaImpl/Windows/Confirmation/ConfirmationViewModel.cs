namespace SparkTrack.AvaloniaImpl.Windows.Confirmation;

using Notification;

public class ConfirmationViewModel(
    string message,
    string? title = null,
    string? acceptText = null,
    string? cancelText = null
) : NotificationViewModel(message, title, acceptText ?? "Да")
{
    public string? CancelText { get; } = cancelText ?? "Нет";
}