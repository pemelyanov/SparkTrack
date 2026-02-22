namespace SparkTrack.AvaloniaImpl.Windows.LinkShare;

using System.Reactive;
using System.Reactive.Disposables;
using Core.Client.Enums;
using Core.Client.Services.PopupNotification;
using NLog;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.Clipboard;
using ViewModels;

public class LinkShareViewModel : DialogViewModelBase
{
    private static readonly ILogger           s_logger = LogManager.GetCurrentClassLogger();
    private readonly        IClipboardService m_clipboardService;

    public LinkShareViewModel(Func<Task<string>> linkFactory, IClipboardService clipboardService, IPopupNotificationService popupNotificationService)
    {
        m_clipboardService = clipboardService;
        GetLinkCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                Link = await linkFactory();
            }
            catch (Exception e)
            {
                s_logger.Error(e, "Error while fetching link");
                popupNotificationService.Show(ENotificationType.Error, e.Message, "Ошибка получения ссылки");
            }
        });
    }

    protected override void OnFirstActivated(CompositeDisposable disposables)
    {
        base.OnFirstActivated(disposables);

        GetLinkCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    [Reactive]
    public string Link { get; private set; } = string.Empty;

    public ReactiveCommand<Unit, Unit> GetLinkCommand { get; }

    public Task CopyAsync() => m_clipboardService.SaveToClipboardAsync(Link, "Ссылка скопирована");
}