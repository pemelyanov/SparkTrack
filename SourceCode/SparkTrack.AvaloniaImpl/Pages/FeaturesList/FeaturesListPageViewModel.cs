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
using Reactive;
using Services.DialogHost;

public class FeaturesListPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly Lazy<IScreen>                                     m_screen;
    private readonly IFeaturesService                                  m_featuresService;
    private readonly Func<Feature, FeaturePageViewModel>               m_featureEditPageViewModelFactory;
    private readonly Func<Guid, FeaturePageViewModel>                  m_featureAddPageViewModelFactory;
    private readonly IDialogService                                    m_dialogService;
    private readonly BehaviorObservableSubject<IReadOnlyList<Feature>> m_selectedFeatures = new([]);

    public FeaturesListPageViewModel(
        Lazy<IScreen> screen,
        IFeaturesService featuresService,
        Func<Feature, FeaturePageViewModel> featureEditPageViewModelFactory,
        Func<Guid, FeaturePageViewModel> featureAddPageViewModelFactory,
        ProjectsFilterViewModel projectsFilterViewModel,
        IDialogService dialogService
    )
    {
        m_screen = screen;
        m_featuresService = featuresService;
        m_featureEditPageViewModelFactory = featureEditPageViewModelFactory;
        m_featureAddPageViewModelFactory = featureAddPageViewModelFactory;
        m_dialogService = dialogService;
        ProjectsFilterViewModel = projectsFilterViewModel;

        ReloadTableCommand = CreateReloadTableCommand();
        var isSelectedPipe = m_selectedFeatures.Select(it => it.Count > 0);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, isSelectedPipe);
        SendOnPaymentCommand = ReactiveCommand.CreateFromTask(SendOnPaymentAsync, isSelectedPipe);
        MarkAsCompletedCommand = ReactiveCommand.CreateFromTask(MarkAsCompletedAsync, isSelectedPipe);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        this.SetupSelectionList(it => it.CurrentPageData, m_selectedFeatures)
            .DisposeWith(disposables);

        ProjectsFilterViewModel.WhenAnyValue(it => it.SelectedProject)
            .CombineLatest(PaginatorViewModel.WhenChanged())
            .CombineLatest(this.WhenAnyValue(it => it.ShowCompleted))
            .CombineLatest(DateRangeViewModel.GetChangingObservable())
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Select(_ => ReloadTableCommand.Execute())
            .Switch()
            .Subscribe()
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
    public bool ShowCompleted { get; set; }

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

    public void CreateFeature()
    {
        if (ProjectsFilterViewModel.SelectedProject is not { } project) return;

        HostScreen.Router.NavigateOnUIThread(m_featureAddPageViewModelFactory(project.Id));
    }

    private ReactiveCommand<Unit, Unit> CreateReloadTableCommand() => ReactiveCommand.CreateFromTask(
        async () =>
        {
            var page = await m_featuresService.GetPageAsync(
                ProjectsFilterViewModel.SelectedProject?.Id,
                ShowCompleted,
                DateRangeViewModel.TryGetStartDate(),
                DateRangeViewModel.TryGetEndDate(),
                PaginatorViewModel.ToQuery()
            );
            
            CurrentPageData = page.Items.Select(it => new SelectableViewModel<Feature>(it)).ToArray();
            PaginatorViewModel.SetPagesQuantity(page.Total);
        }
    );

    private async Task DeleteAsync()
    {
        if (m_selectedFeatures.Value.Count == 0) return;

        if (!await m_dialogService.ConfirmAsync(
            $"Вы уверены что хотите удалить выбранные идеи ({m_selectedFeatures.Value.Count})? Идеи имеющие связь с оплаченными задачами будут добавлены в архив, остальные будут полностью удалены.",
            "Удаление пользователей"
        )) return;

        var errorsList = new List<(Exception exception, Feature feature)>();

        foreach (var feature in m_selectedFeatures.Value)
        {
            try
            {
                await m_featuresService.DeleteAsync(feature.Id);
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