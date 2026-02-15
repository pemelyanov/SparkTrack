using Avalonia.Controls;

namespace SparkTrack.AvaloniaImpl.Controls.Link;

using System.Diagnostics;
using Avalonia;
using Avalonia.Input;

public partial class Link : UserControl
{
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

    private void InputElement_OnTapped(object? sender, TappedEventArgs e)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = Url,
                UseShellExecute = true
            }
        );
    }
}