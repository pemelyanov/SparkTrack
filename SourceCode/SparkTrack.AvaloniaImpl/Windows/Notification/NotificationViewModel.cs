namespace SparkTrack.AvaloniaImpl.Windows.Notification;

using ViewModels;

public class NotificationViewModel(string message,
                                   string? title = null,
                                   string? acceptText = null) 
    : DialogViewModelBase
{
    public string Message { get; } = message;

    public string? Title { get; } = title;

    public string AcceptText { get; } = acceptText ?? "Ок";
}