namespace SparkTrack.AvaloniaImpl.Extensions;

using System.Linq.Expressions;
using System.Reactive.Linq;
using ReactiveUI;
using ViewModels;

public static class SelectableViewModelExtensions
{
    public static IDisposable SetupSelectionList<TViewModel, TModel>(
        this TViewModel viewModel,
        Expression<Func<TViewModel, IReadOnlyList<SelectableViewModel<TModel>>>> selector,
        IObserver<IReadOnlyList<TModel>> selectionList
    ) where TViewModel : ReactiveObject => viewModel.WhenAnyValue(selector)
        .Select(list => list.Count == 0
            ? Observable.Return<IList<SelectableViewModel<TModel>>>([])
            : list.Select(it => it.WhenAnyValue(v => v.IsSelected)
                    .Select(_ => it
                    )
                )
                .CombineLatest()
        )
        .Switch()
        .Select(list => list.Where(it => it.IsSelected).Select(it => it.Model).ToArray())
        .Throttle(TimeSpan.FromMilliseconds(50))
        .ObserveOn(RxApp.MainThreadScheduler)
        .Subscribe(selectionList);
}