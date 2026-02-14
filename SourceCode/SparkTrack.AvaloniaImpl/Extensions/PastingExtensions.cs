namespace SparkTrack.AvaloniaImpl.Extensions;

using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using NLog;

public static class PastingExtensions
{
    private static readonly ILogger s_logger      = LogManager.GetCurrentClassLogger();
    private static readonly Regex   s_imageFormat = new(@"(?i)\b(?<ext>jpe?g|png|JPE?G|PNG)\b");
    
    public static async Task HandleImagePastingFromClipboard(this Control control, Action<byte[], string> imageCallback)
    {
        s_logger.Info("Handling pasting from clipboard by {control}...", control);
        
        var clipboard = TopLevel.GetTopLevel(control)?.Clipboard;

        if (clipboard?.TryGetDataAsync() is not { } dataTask) return;
        
        var data = await dataTask;
            
        if(data is null) return;

        foreach (var asyncDataTransferItem in data.Items)
        {
            var imageFormats = asyncDataTransferItem.Formats.Where(it =>
                s_imageFormat.IsMatch(it.Identifier) || it is DataFormat<byte[]>
            );

            foreach (var imageFormat in imageFormats)
            {
                var match = s_imageFormat.Match(imageFormat.Identifier);

                var imageBytes = await asyncDataTransferItem.TryGetRawAsync(imageFormat) as byte[];
                
                if(imageBytes is null) continue;

                var ext = match.Groups.TryGetValue("ext", out var group)
                    ? group.Value
                    : imageBytes.GetImageExtensionBySignature();

                if (ext is null)
                {
                    s_logger.Warn("Cannot determine format of pasting source");
                
                    continue;
                }
            
                imageCallback(imageBytes, ext.ToLower());
                return;
            }
        }
    }
}