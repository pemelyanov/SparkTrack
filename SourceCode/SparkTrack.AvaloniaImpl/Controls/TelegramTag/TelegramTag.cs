namespace SparkTrack.AvaloniaImpl.Controls.TelegramTag;

using Avalonia.Controls;
using Avalonia.Input;
using System.Diagnostics;

public class TelegramTag : TextBlock
{
    public TelegramTag()
    {
        Tapped += OnTapped;
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        bool isTgLaunched;

        var userId = Text?.TrimStart('@');
        
        if(string.IsNullOrWhiteSpace(userId)) return;
        
        try
        {
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
        }

        if (isTgLaunched) return;

        string webUrl = $"https://t.me/{userId}";
        Process.Start(
            new ProcessStartInfo
            {
                FileName = webUrl,
                UseShellExecute = true
            }
        );
    }
}