namespace SparkTrack.AvaloniaImpl.Services.Clipboard;

public interface IClipboardService
{
    Task SaveToClipboardAsync(string text, string? notificationText = null);
}