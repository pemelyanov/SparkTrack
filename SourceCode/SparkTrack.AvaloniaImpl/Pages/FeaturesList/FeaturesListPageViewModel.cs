namespace SparkTrack.AvaloniaImpl.Pages.FeaturesList;

using Controls.ProjectsFilter;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Extensions;
using Feature;
using ViewModels;
using Core.Shared.Data;
using Core.Shared.Data.Entities;
using SparkTrack.Core.Shared.Services.Features;
using System.Reactive;
using System.Reactive.Disposables;

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

        ReloadTableCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    public string UrlPathSegment => "features";

    public IScreen HostScreen => m_screen.Value;

    public ProjectsFilterViewModel ProjectsFilterViewModel { get; }

    [Reactive]
    public IReadOnlyList<SelectableViewModel<Feature>> CurrentPageData { get; private set; } = [];

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
                true,
                PageQuery.All
            );

            CurrentPageData = page.Items.Select(it => new SelectableViewModel<Feature>(it)).ToArray();
        }
    );
}