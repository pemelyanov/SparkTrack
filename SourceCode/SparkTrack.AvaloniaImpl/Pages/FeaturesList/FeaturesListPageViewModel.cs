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

public class FeaturesListPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly Lazy<IScreen>                       m_screen;
    private readonly IFeaturesService                    m_featuresService;
    private readonly Func<Feature, FeaturePageViewModel> m_featureEditPageViewModelFactory;
    private readonly Func<Guid, FeaturePageViewModel> m_featureAddPageViewModelFactory;

    public FeaturesListPageViewModel(
        Lazy<IScreen> screen,
        IFeaturesService featuresService,
        Func<Feature, FeaturePageViewModel> featureEditPageViewModelFactory,
        Func<Guid, FeaturePageViewModel> featureAddPageViewModelFactory,
        ProjectsFilterViewModel projectsFilterViewModel
    )
    {
        m_screen = screen;
        m_featuresService = featuresService;
        m_featureEditPageViewModelFactory = featureEditPageViewModelFactory;
        m_featureAddPageViewModelFactory = featureAddPageViewModelFactory;
        ProjectsFilterViewModel = projectsFilterViewModel;

        ReloadTableCommand = CreateReloadTableCommand();
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

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
}