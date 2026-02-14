namespace SparkTrack.AvaloniaImpl.Windows.Confirmation;

using Notification;
using ViewModels;

public class ConfirmationViewModel(
    string message,
    string? title = null,
    string? acceptText = null,
    string? cancelText = null,
    IReadOnlyList<SelectableViewModel<string>>? additionalOptionsList = null
) : NotificationViewModel(message, title, acceptText ?? "Да")
{
    public string? CancelText { get; } = cancelText ?? "Нет";

    public IReadOnlyList<SelectableViewModel<string>> AdditionalOptionsList { get; } = additionalOptionsList ?? [];
}