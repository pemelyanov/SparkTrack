using Avalonia.Controls;

namespace SparkTrack.AvaloniaImpl.Controls.Link;

using System.Diagnostics;
using Avalonia;
using Avalonia.Input;
using NLog;
using Services.Clipboard;
using Splat;
using ILogger = NLog.ILogger;

public partial class Link : UserControl
{
    private static readonly ILogger           s_logger           = LogManager.GetCurrentClassLogger();
    private readonly        IClipboardService m_clipboardService = Locator.Current.GetService<IClipboardService>()!;
    
    public Link()
    {
        InitializeComponent();
    }

    #region Text Property

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<Link, string?>(nameof(Text));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    #endregion

    #region Url Property

    public static readonly StyledProperty<string?> UrlProperty =
        AvaloniaProperty.Register<Link, string?>(nameof(Url));

    public string? Url
    {
        get => GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    #endregion

    #region DisplayText Property

    public static readonly StyledProperty<string?> DisplayTextProperty =
        AvaloniaProperty.Register<Link, string?>(nameof(DisplayText));

    public string? DisplayText
    {
        get => GetValue(DisplayTextProperty);
        private set => SetValue(DisplayTextProperty, value);
    }

    #endregion

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        
        if(change.Property != TextProperty && change.Property != UrlProperty) return;

        DisplayText = Text ?? Url;
    }

    private void InputElement_OnTapped(object? sender, TappedEventArgs args)
    {
        try
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = Url,
                    UseShellExecute = true
                }
            );
        }
        catch (Exception e)
        {
            s_logger.Warn(e, "Cannot start process for {url}", Url);
        }
        
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if(sender is not Control control) return;
        
        var properties = e.GetCurrentPoint(control).Properties;
        
        if(!properties.IsRightButtonPressed || string.IsNullOrEmpty(Url)) return;

        m_clipboardService.SaveToClipboardAsync(Url, "Ссылка скопирована");
    }
}