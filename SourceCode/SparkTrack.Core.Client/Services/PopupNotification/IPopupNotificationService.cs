namespace SparkTrack.Core.Client.Services.PopupNotification;

using Enums;

public interface IPopupNotificationService
{
    void Show(ENotificationType type, string message, string? title = null);
}