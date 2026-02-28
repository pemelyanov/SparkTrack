namespace SparkTrack.AvaloniaImpl.Extensions;

using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData.Binding;

public static class ObservableExtensions
{
    public static IObservable<IReadOnlyList<TData>> GetListObservable<TData>(this ObservableCollection<TData> list) => list
        .ObserveCollectionChanges()
        .Select(_ => list as IReadOnlyList<TData>)
        .StartWith(list);
}