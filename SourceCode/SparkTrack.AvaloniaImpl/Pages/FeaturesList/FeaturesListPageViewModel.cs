using NLog;
using SparkTrack.AvaloniaImpl.Data.Configurations;
using SparkTrack.Core.Client.Enums;
using SparkTrack.Core.Client.Extensions;
using SparkTrack.Core.Client.Services.Configuration;
using SparkTrack.Core.Client.Services.PopupNotification;

namespace SparkTrack.AvaloniaImpl.Pages.FeaturesList;

using Controls.ProjectsFilter;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Extensions;
using Feature;
using ViewModels;
using Core.Shared.Data.Entities;
using SparkTrack.Core.Shared.Services.Features;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using ConfirmationOptions;
using Controls.TemplateSelectionForm;
using Core.Shared.Data;
using Data.Templates;
using Reactive;
using Services.DialogHost;

public class FeaturesListPageViewModel : ViewModelBase, IRoutableViewModel
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    private readonly Lazy<IScreen>                                         m_screen;
    private readonly IFeaturesService                                      m_featuresService;
    private readonly Func<Feature, FeaturePageViewModel>                   m_featureEditPageViewModelFactory;
    private readonly Func<Project, FeaturePageViewModel>                   m_featureAddPageViewModelFactory;
    private readonly IDialogService                                        m_dialogService;
    private readonly Func<TemplateSelectionFormViewModel<FeatureTemplate>> m_templateSelectionViewModelFactory;
    private readonly IConfigurationService<FeaturesPageConfig>             m_featuresPageConfig;
    private readonly IPopupNotificationService                             m_popupNotificationService;
    private readonly BehaviorObservableSubject<IReadOnlyList<Feature>>     m_selectedFeatures = new([]);
    private readonly BehaviorObservableSubject<FeatureFilterQuery?>        m_filterQuery      = new(null);
    private          Guid?                                                 m_projectToSelectOnNextLoad;

    public FeaturesListPageViewModel(
        Lazy<IScreen> screen,
        IFeaturesService featuresService,
        Func<Feature, FeaturePageViewModel> featureEditPageViewModelFactory,
        Func<Project, FeaturePageViewModel> featureAddPageViewModelFactory,
        ProjectsFilterViewModel projectsFilterViewModel,
        IDialogService dialogService,
        Func<TemplateSelectionFormViewModel<FeatureTemplate>> templateSelectionViewModelFactory,
        IConfigurationService<FeaturesPageConfig> featuresPageConfig,
        IPopupNotificationService popupNotificationService
    )
    {
        m_screen = screen;
        m_featuresService = featuresService;
        m_featureEditPageViewModelFactory = featureEditPageViewModelFactory;
        m_featureAddPageViewModelFactory = featureAddPageViewModelFactory;
        m_dialogService = dialogService;
        m_templateSelectionViewModelFactory = templateSelectionViewModelFactory;
        m_featuresPageConfig = featuresPageConfig;
        m_popupNotificationService = popupNotificationService;
        ProjectsFilterViewModel = projectsFilterViewModel;

        ReloadTableCommand = CreateReloadTableCommand();
        var isSelectedPipe = m_selectedFeatures.Select(it => it.Count > 0);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, isSelectedPipe);
        SendOnPaymentCommand = ReactiveCommand.CreateFromTask(SendOnPaymentAsync, isSelectedPipe);
        MarkAsCompletedCommand = ReactiveCommand.CreateFromTask(MarkAsCompletedAsync, isSelectedPipe);

        if (featuresPageConfig.Config.ShowOnlyMine is { } showOnlyMine) ShowOnlyMine = showOnlyMine;

        if (featuresPageConfig.Config.ItemsPerPage is { } itemsPerPage) PaginatorViewModel.ItemsPerPage = itemsPerPage;

        if (featuresPageConfig.Config.IsDatesFilterEnabled is { } isDatesFilterEnabled)
            DateRangeViewModel.IsSelected = isDatesFilterEnabled;

        if (featuresPageConfig.Config.Filters is { } filters)
        {
            m_filterQuery.Value = filters;

            ShowClosed = filters.ShowClosed;
            ShowCompleted = filters.ShowCompleted;
            DateRangeViewModel.Model.StartDate = filters.StartDate;
            DateRangeViewModel.Model.EndDate = filters.EndDate;
            m_projectToSelectOnNextLoad = filters.ProjectId;
        }
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        this.SetupSelectionList(it => it.CurrentPageData, m_selectedFeatures)
            .DisposeWith(disposables);

        ProjectsFilterViewModel.WhenAnyValue(it => it.SelectedProject)
            .CombineLatest(this.WhenAnyValue(it => it.ShowClosed))
            .CombineLatest(this.WhenAnyValue(it => it.ShowCompleted))
            .CombineLatest(DateRangeViewModel.GetChangingObservable())
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Select(_ => new FeatureFilterQuery
            {
                ProjectId = ProjectsFilterViewModel.SelectedProject?.Id,
                EndDate = DateRangeViewModel.TryGetEndDate(),
                StartDate = DateRangeViewModel.TryGetStartDate(),
                ShowClosed = ShowClosed,
                ShowCompleted = ShowCompleted,
            })
            .Subscribe(m_filterQuery)
            .DisposeWith(disposables);

        m_filterQuery
            .CombineLatest(PaginatorViewModel.WhenChanged())
            .CombineLatest(this.WhenAnyValue(it => it.ShowOnlyMine))
            .Select(_ => ReloadTableCommand.Execute())
            .Switch()
            .Subscribe()
            .DisposeWith(disposables);

        m_filterQuery.Skip(1)
            .Subscribe(value => m_featuresPageConfig.Update(it => it with
            {
                Filters = value
            }))
            .DisposeWith(disposables);

        this.WhenAnyValue(it => it.ShowOnlyMine).Skip(1)
            .Subscribe(value => m_featuresPageConfig.Update(it => it with
            {
                ShowOnlyMine = value
            }))
            .DisposeWith(disposables);

        DateRangeViewModel.WhenAnyValue(it => it.IsSelected)
            .Subscribe(value => m_featuresPageConfig.Update(it => it with
            {
                IsDatesFilterEnabled = value
            }))
            .DisposeWith(disposables);

        PaginatorViewModel.WhenAnyValue(it => it.ItemsPerPage)
            .Skip(1)
            .Subscribe(value => m_featuresPageConfig.Update(it => it with
            {
                ItemsPerPage = value
            }))
            .DisposeWith(disposables);

        ProjectsFilterViewModel.WhenAnyValue(it => it.ProjectsList)
            .Subscribe(list =>
            {
                if (m_projectToSelectOnNextLoad is null ||
                    list.FirstOrDefault(it => it.Id == m_projectToSelectOnNextLoad) is not { } project) return;

                m_projectToSelectOnNextLoad = null;
                ProjectsFilterViewModel.SelectedProject = project;
            })
            .DisposeWith(disposables);
    }

    public string UrlPathSegment => "features";

    public IScreen HostScreen => m_screen.Value;

    public ProjectsFilterViewModel ProjectsFilterViewModel { get; }

    public SelectableViewModel<DateRangeViewModel> DateRangeViewModel { get; } = new(new DateRangeViewModel())
    {
        IsSelected = true
    };

    [Reactive]
    public bool ShowClosed { get; set; }

    [Reactive]
    public bool ShowCompleted { get; set; } = true;

    [Reactive]
    public bool ShowOnlyMine { get; set; } = true;

    [Reactive]
    public IReadOnlyList<SelectableViewModel<Feature>> CurrentPageData { get; private set; } = [];

    public PaginatorViewModel PaginatorViewModel { get; } = new();

    public ReactiveCommand<Unit, Unit> ReloadTableCommand { get; }

    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    public ReactiveCommand<Unit, Unit> SendOnPaymentCommand { get; }

    public ReactiveCommand<Unit, Unit> MarkAsCompletedCommand { get; }

    public void OpenFeature(Feature feature)
    {
        HostScreen.Router.NavigateOnUIThread(m_featureEditPageViewModelFactory(feature));
    }

    public async Task CreateFeatureFromTemplateAsync()
    {
        if (ProjectsFilterViewModel.SelectedProject is not { } project) return;

        var selectionViewModel = m_templateSelectionViewModelFactory();

        if (await m_dialogService.ShowAsync(selectionViewModel) is not true ||
            selectionViewModel.SelectedTemplate is not FeatureTemplate template) return;

        var featureViewModel = m_featureAddPageViewModelFactory(project);
        await featureViewModel.InitializeFromTemplateAsync(template);

        HostScreen.Router.NavigateOnUIThread(featureViewModel);
    }

    public void CreateFeature()
    {
        if (ProjectsFilterViewModel.SelectedProject is not { } project) return;

        HostScreen.Router.NavigateOnUIThread(m_featureAddPageViewModelFactory(project));
    }

    private ReactiveCommand<Unit, Unit> CreateReloadTableCommand() => ReactiveCommand.CreateFromTask(async () =>
        {
            try
            {
                var sort = new SortQuery("CreatedAt", true);
                var pagination = PaginatorViewModel.ToQuery();
                
                s_logger.Info(
                    "Loading features page. showOnlyMine: {showOnlyMine}; filter: {filter}; sort: {sort}; pagination: {pagination}",
                    ShowOnlyMine, m_filterQuery.Value, sort, pagination);

                var page = await m_featuresService.GetPageAsync(
                    ShowOnlyMine,
                    m_filterQuery.Value,
                    sort,
                    pagination
                );

                CurrentPageData = page.Items.Select(it => new SelectableViewModel<Feature>(it)).ToArray();
                PaginatorViewModel.SetPagesQuantity(page.Total);
            }
            catch (Exception e)
            {
                s_logger.Warn(e);

                m_popupNotificationService.Show(ENotificationType.Error, e.Message, "Ошибка загрузки страницы");
            }
        }
    );

    private async Task DeleteAsync()
    {
        if (m_selectedFeatures.Value.Count == 0) return;

        var forceOption = new ForceDeleteOption();

        if (!await m_dialogService.ConfirmAsync(
                $"Вы уверены что хотите удалить выбранные идеи ({m_selectedFeatures.Value.Count})? Идеи имеющие связь с оплаченными задачами будут добавлены в архив, остальные будут полностью удалены.",
                "Удаление пользователей",
                additionalOptionsList: [forceOption]
            )) return;

        var errorsList = new List<(Exception exception, Feature feature)>();

        foreach (var feature in m_selectedFeatures.Value)
        {
            try
            {
                await m_featuresService.DeleteAsync(feature.Id, forceOption.IsSelected);
            }
            catch (Exception e)
            {
                errorsList.Add((e, feature));
            }
        }

        if (errorsList.Count != 0)
        {
            await m_dialogService.NotifyAsync(
                $"{string.Join(";\n\n\n", errorsList.Select(it => $"{it.feature.Name}: {it.exception.Message}"))}.",
                "При удалении некоторых идей возникли ошибки"
            );
        }

        await ReloadTableCommand.Execute().ToTask();
    }

    private async Task SendOnPaymentAsync()
    {
        await m_dialogService.NotifyAsync("W.I.P");
    }

    private async Task MarkAsCompletedAsync()
    {
        await m_dialogService.NotifyAsync("W.I.P");
    }
}