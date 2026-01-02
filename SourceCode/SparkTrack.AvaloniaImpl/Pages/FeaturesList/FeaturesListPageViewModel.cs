namespace SparkTrack.AvaloniaImpl.Pages.FeaturesList;

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
using System.Reactive.Linq;

public class FeaturesListPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly Lazy<IScreen>                        m_screen;
    private readonly IFeaturesService                     m_featuresService;
    private readonly Func<Feature?, FeaturePageViewModel> m_featurePageViewModelFactory;

    public FeaturesListPageViewModel(Lazy<IScreen> screen, IFeaturesService featuresService, Func<Feature?, FeaturePageViewModel> featurePageViewModelFactory)
    {
        m_screen = screen;
        m_featuresService = featuresService;
        m_featurePageViewModelFactory = featurePageViewModelFactory;

        ReloadTableCommand = CreateReloadTableCommand();
    }

    protected override void OnFirstActivated(CompositeDisposable disposables)
    {
        base.OnFirstActivated(disposables);
        
        SetupItemSelectionChangedReaction(disposables);
        SetupTableSelectionStateChangeReaction(disposables);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);
        
        ReloadTableCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    public string UrlPathSegment => "features";

    public IScreen HostScreen => m_screen.Value;
    
    [Reactive]
    public bool? CurrentPageSelectionState { get; set; }

    [Reactive]
    public IReadOnlyList<SelectableViewModel<Feature>> CurrentPageData { get; private set; } = [];
    
    public ReactiveCommand<Unit, Unit> ReloadTableCommand { get; }

    public void OpenFeature(Feature feature)
    {
        HostScreen.Router.NavigateOnUIThread(m_featurePageViewModelFactory(feature));
    }

    public void CreateFeature()
    {
        HostScreen.Router.NavigateOnUIThread(m_featurePageViewModelFactory(null));
    }

    private ReactiveCommand<Unit, Unit> CreateReloadTableCommand() => ReactiveCommand.CreateFromTask(
        async () =>
        {
            var page = await m_featuresService.GetPageAsync(null, true, PageQuery.All);

            CurrentPageData = page.Items.Select(it => new SelectableViewModel<Feature>(it)).ToArray();
        }
    );
    
    private void SetupTableSelectionStateChangeReaction(CompositeDisposable disposables)
    {
        this.WhenAnyValue(vm => vm.CurrentPageSelectionState)
            .Where(it => it is not null)
            .Subscribe(
                state =>
                {
                    foreach (var item in CurrentPageData)
                        item.IsSelected = state is true;
                }
            )
            .DisposeWith(disposables);
    }
    
    private void SetupItemSelectionChangedReaction(CompositeDisposable disposables)
    {
        this.WhenAnyValue(it => it.CurrentPageData)
            .Select(
                list => list.Count == 0
                    ? Observable.Return(Array.Empty<bool>())
                    : list.Select(it => it.WhenAnyValue(vm => vm.IsSelected)).CombineLatest()
            )
            .Switch()
            .Select<IList<bool>, bool?>(
                selectionList =>
                {
                    if (selectionList.Count == 0) return false;

                    var selectedQuantity = 0;
                    var unselectedQuantity = 0;

                    foreach (bool isSelected in selectionList)
                        if (isSelected)
                            selectedQuantity++;
                        else unselectedQuantity++;

                    if (selectedQuantity == selectionList.Count) return true;
                    if (unselectedQuantity == selectionList.Count) return false;

                    return null;
                }
            )
            .Subscribe(value => CurrentPageSelectionState = value)
            .DisposeWith(disposables);
    }
}