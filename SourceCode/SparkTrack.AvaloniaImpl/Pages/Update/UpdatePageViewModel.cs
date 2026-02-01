namespace SparkTrack.AvaloniaImpl.Pages.Update;

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Authorization;
using Core.Client.Enums;
using Core.Client.Services.PopupNotification;
using Extensions;
using Fanatiki.Loading.Data;
using Fanatiki.MVVM.ViewModels;
using Fanatiki.Updating.Services;
using NLog;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

public class UpdatePageViewModel : ViewModelBase, IRoutableViewModel
{
    #region Fields

    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    private readonly IUpdateService                   m_updateService;
    private readonly IPopupNotificationService        m_popupNotificationService;
    private readonly Lazy<IScreen>                    m_hostScreen;
    private readonly Func<AuthorizationPageViewModel> m_firstPageFactory;

    #endregion

    #region LifeCycle

    public UpdatePageViewModel(
        IUpdateService updateService,
        IPopupNotificationService popupNotificationService,
        Lazy<IScreen> hostScreen,
        Func<AuthorizationPageViewModel> firstPageFactory
    )
    {
        m_updateService = updateService;
        m_popupNotificationService = popupNotificationService;
        m_hostScreen = hostScreen;
        m_firstPageFactory = firstPageFactory;

        TryUpdateCommand = CreateTryUpdateAsync();
    }

    protected override void OnFirstActivated(CompositeDisposable disposables)
    {
        base.OnFirstActivated(disposables);

        IObservable<LoadingProgress> progress = TryUpdateCommand.Execute()
            .Replay(1)
            .RefCount();

        LoadingProgress = progress;

        progress.Subscribe().DisposeWith(disposables);
    }

    #endregion

    #region Properties

    public string UrlPathSegment => "update";

    public IScreen HostScreen => m_hostScreen.Value;

    [Reactive]
    public IObservable<LoadingProgress>? LoadingProgress { get; private set; } = Observable.Return(
        new LoadingProgress
        {
            StageName = "Проверка обновлений...",
        }
    );

    private ReactiveCommand<Unit, LoadingProgress> TryUpdateCommand { get; }

    #endregion

    #region Methods

    private ReactiveCommand<Unit, LoadingProgress> CreateTryUpdateAsync() =>
        ReactiveCommand.CreateFromObservable<Unit, LoadingProgress>(_ =>
            Observable.Create<LoadingProgress>(async observer =>
                {
                    try
                    {
                        bool needClose = await m_updateService.TryUpdateAsync(observer);
                        
                        if (needClose)
                        {
                            Environment.Exit(0);
                            return;
                        }
                        
                        HostScreen.Router.NavigateOnUIThread(m_firstPageFactory());
                    }
                    catch (Exception e)
                    {
                        s_logger.Error(e);
                        m_popupNotificationService.Show(
                            ENotificationType.Error,
                            "При обновлении возникли ошибки. Повторная попытка обновления будет выполнена при следующем запуске",
                            "Ошибка обновления"
                        );
                    }
                }
            )
        );

    #endregion
}