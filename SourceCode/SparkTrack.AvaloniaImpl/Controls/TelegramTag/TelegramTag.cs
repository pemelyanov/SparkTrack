namespace SparkTrack.AvaloniaImpl.Controls.TelegramTag;

using Avalonia.Controls;
using Avalonia.Input;
using System.Diagnostics;
using NLog;

public class TelegramTag : TextBlock
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    
    public TelegramTag()
    {
        Tapped += OnTapped;
    }

    private void OnTapped(object? sender, TappedEventArgs args)
    {
        bool isTgLaunched;

        var userId = Text?.TrimStart('@');
        
        if(string.IsNullOrWhiteSpace(userId)) return;
        
        try
        {
            s_logger.Info("Openning telegram app with user {user}", userId);
            var tgUrl = $"tg://resolve?domain={userId}";
            isTgLaunched = Process.Start(
                new ProcessStartInfo
                {
                    FileName = tgUrl,
                    UseShellExecute = true
                }
            ) is not null;
        }
        catch
        {
            isTgLaunched = false;
            s_logger.Warn("Cannot launch telegram app");
        }

        if (isTgLaunched)
        {
            args.Handled = true;
            return;
        }

        s_logger.Info("Openning telegram in browser with user {user}", userId);
        string webUrl = $"https://t.me/{userId}";

        try
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = webUrl,
                    UseShellExecute = true
                }
            );
            args.Handled = true;
        }
        catch (Exception e)
        {
            s_logger.Warn(e, "Telegram tag open failed");
        }
    }
}