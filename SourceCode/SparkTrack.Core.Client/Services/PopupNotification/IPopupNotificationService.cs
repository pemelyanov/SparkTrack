namespace SparkTrack.Core.Client.Services.PopupNotification;

public interface IPopupNotificationService
{
    void Notification(string message, string? title = null);

    void Error(string message, string? title = null);
}