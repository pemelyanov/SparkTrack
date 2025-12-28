namespace SparkTrack.AvaloniaImpl.Windows.Main;

using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Core.Client.Services.PopupNotification;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Services.DialogHost;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>, IDialogHost, IPopupNotificationService
{
    private readonly WindowNotificationManager m_notificationManager;
    
    public MainWindow()
    {
        InitializeComponent();

        m_notificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.BottomRight
        };
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

    public void Notification(string message, string? title = null)
    {
        m_notificationManager.Show(CreateNotification(message, title, NotificationType.Information));
    }

    public void Error(string message, string? title = null)
    {
        m_notificationManager.Show(CreateNotification(message, title, NotificationType.Error));
    }

    private Notification CreateNotification(string message, string? title, NotificationType type) => new Notification
    {
        Title = title,
        Message = message,
        Type = type
    };
}