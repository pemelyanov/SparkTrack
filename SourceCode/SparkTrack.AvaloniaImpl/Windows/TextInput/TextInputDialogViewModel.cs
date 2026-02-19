using ReactiveUI.Fody.Helpers;
using SparkTrack.AvaloniaImpl.ViewModels;
using SparkTrack.AvaloniaImpl.Windows.Confirmation;

namespace SparkTrack.AvaloniaImpl.Windows.TextInput;

public class TextInputDialogViewModel(string message, string? title = null, string? acceptText = null, string? cancelText = null, IReadOnlyList<SelectableViewModel<string>>? additionalOptionsList = null) : ConfirmationViewModel(message, title, acceptText, cancelText, additionalOptionsList)
{
    [Reactive]
    public string Text { get; set; }
}