namespace SparkTrack.AvaloniaImpl.Extensions;

using Avalonia.Controls;
using Clowd.Clipboard;
using NLog;

public static class PastingExtensions
{
    private static readonly ILogger s_logger      = LogManager.GetCurrentClassLogger();
    
    public static async Task HandleImagePastingFromClipboard(this Control control, Action<byte[], string> imageCallback)
    {
        s_logger.Info("Handling pasting from clipboard by {control}...", control);

        if (!SimpleClipboard.ContainsImage())
        {
            s_logger.Info("No images in clipboard");
            return;
        }
        
        var bitmap = SimpleClipboard.GetImage();
        
        s_logger.Warn("Cant parse image from clibloard, fetching details...");

        if (bitmap is null)
        {
            var clipboard = TopLevel.GetTopLevel(control)?.Clipboard;

            if (clipboard?.TryGetDataAsync() is not { } dataTask)
            {
                s_logger.Warn("Cannot get any data from clipboard");
                
                return;
            }
        
            var data = await dataTask;

            if (data is null)
            {
                s_logger.Warn("Data in clipboard was null");
                return;
            }

            var formatsInClipboard = data.Formats.Select(it => $"Kind: {it.Kind}; Identifier: {it.Identifier}");
            
            s_logger.Warn("Parsed formats: {formats}", string.Join(", ", formatsInClipboard));
            
            return;
        }

        var extension = bitmap.GetImageExtensionBySignature();

        if (extension is null)
        {
            s_logger.Warn("Cannot determine pasting image extension. Falling back to \"png\"");
            extension = "png";
        }
        
        s_logger.Info("Image successfully parsed from cliboard. Weight: {weight}, extension: {extension}", bitmap.Length, extension);
        imageCallback(bitmap, extension);
    }
}