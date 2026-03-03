using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using SparkTrack.AvaloniaImpl.Data.Configurations;
using SparkTrack.Core.Client.Extensions;
using SparkTrack.Core.Client.Services.Configuration;
using Splat;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace SparkTrack.AvaloniaImpl.Windows.Main;

using API.MappingExtensions;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using Core.Client.Enums;
using Core.Client.Services.PopupNotification;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Services.Clipboard;
using Services.DialogHost;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>, IDialogService, IPopupNotificationService,
    IClipboardService
{
    private readonly WindowNotificationManager m_notificationManager;

    private readonly IConfigurationService<InterfaceConfiguration> m_interfaceConfiguration =
        Locator.Current.GetService<IConfigurationService<InterfaceConfiguration>>()!;

    private readonly IConfigurationService<WindowStateConfig> m_windowStateConfig =
        Locator.Current.GetService<IConfigurationService<WindowStateConfig>>()!;

    private readonly BehaviorSubject<Point?> m_windowPosition = new(null);
    private readonly BehaviorSubject<Size?>  m_windowSize     = new(null);

    private readonly double m_scale;

    public MainWindow()
    {
        InitializeComponent();

        m_scale = m_interfaceConfiguration.Config.Scale / 100d;

        m_notificationManager = new WindowNotificationManager
        {
            Position = NotificationPosition.BottomCenter,
            ZIndex = 1000
        };

        this.WhenActivated(OnActivated);

        if (m_windowStateConfig.Config.Position is { } position)
            Position = new PixelPoint(position.X, position.Y);

        if (m_windowStateConfig.Config.Size is { } size)
        {
            Width = size.Width;
            Height = size.Height;
        }
    }

    private void OnActivated(CompositeDisposable disposables)
    {
        m_windowPosition.CombineLatest(m_windowSize, (position, size) => new { position, size })
            .Throttle(TimeSpan.FromSeconds(1))
            .Skip(1)
            .Subscribe(value => m_windowStateConfig.Update(it => it with
            {
                Position = value.position,
                Size = value.size
            }))
            .DisposeWith(disposables);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        InitializeWindowNotificationManagerViaOverlay();
    }

    #region ContentWidth

    public static readonly StyledProperty<double> ContentWidthProperty =
        AvaloniaProperty.Register<MainWindow, double>(
            nameof(ContentWidth), defaultValue: Double.NaN);

    public double ContentWidth
    {
        get => GetValue(ContentWidthProperty);
        set => SetValue(ContentWidthProperty, value);
    }

    #region ContentHeight

    public static readonly StyledProperty<double> ContentHeightProperty =
        AvaloniaProperty.Register<MainWindow, double>(
            nameof(ContentHeight), defaultValue: Double.NaN);

    public double ContentHeight
    {
        get => GetValue(ContentHeightProperty);
        set => SetValue(ContentHeightProperty, value);
    }

    #endregion

    #endregion

    private void InitializeWindowNotificationManagerViaOverlay()
    {
        var visualLayerManager = this.FindDescendantOfType<VisualLayerManager>();

        if (visualLayerManager?.OverlayLayer is null ||
            visualLayerManager.OverlayLayer.Children.Contains(m_notificationManager)) return;

        visualLayerManager.OverlayLayer.Children.Add(m_notificationManager);
    }

    private void NavigationItem_OnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Control { Tag: Type type }) return;

        ViewModel?.SelectPage(type);
    }

    public async Task<bool?> ShowAsync(ReactiveObject viewModel)
    {
        var view = ViewLocator.Current.ResolveView(viewModel);

        if (view != null) view.ViewModel = viewModel;

        var result = await (view switch
        {
            ContentDialog contentDialog => contentDialog.ShowAsync(this),
            _ => throw new NotSupportedException()
        });

        return ToBool(result);
    }

    private bool? ToBool(ContentDialogResult result) => result switch
    {
        ContentDialogResult.Primary => true,
        _ => null
    };

    public void Show(ENotificationType type, string message, string? title = null)
    {
        m_notificationManager.Show(CreateNotification(message, title, type.Cast<NotificationType>()));
    }

    private Notification CreateNotification(string message, string? title, NotificationType type) => new()
    {
        Title = title,
        Message = message,
        Type = type
    };

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ContentWidth = SizeBox.Bounds.Width / m_scale;
        ContentHeight = SizeBox.Bounds.Height / m_scale;
    }

    public async Task SaveToClipboardAsync(string text, string? notificationText = null)
    {
        if (Clipboard is null) return;

        await Clipboard.SetTextAsync(text);

        if (!string.IsNullOrEmpty(notificationText)) Show(ENotificationType.Information, notificationText);
    }

    private void Window_OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        m_windowPosition.OnNext(new Point(Position.X, Position.Y));
    }

    private void Window_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        m_windowSize.OnNext(new Size((int)Bounds.Width, (int)Bounds.Height));
    }
}