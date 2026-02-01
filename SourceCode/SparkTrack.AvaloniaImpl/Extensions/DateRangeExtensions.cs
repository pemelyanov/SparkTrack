namespace SparkTrack.AvaloniaImpl.Extensions;

using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using ViewModels;

public static class DateRangeExtensions
{
    public static DateTime? TryGetStartDate(this SelectableViewModel<DateRangeViewModel> dateRange) =>
        dateRange.IsSelected ? dateRange.Model.StartDate : null;

    public static DateTime? TryGetEndDate(this SelectableViewModel<DateRangeViewModel> dateRange) =>
        dateRange.IsSelected ? dateRange.Model.EndDate : null;

    public static IObservable<Unit> GetChangingObservable(this SelectableViewModel<DateRangeViewModel> dateRange) =>
        dateRange.WhenAnyValue(it => it.IsSelected)
            .CombineLatest(dateRange.Model.WhenAnyValue(it => it.StartDate))
            .CombineLatest(dateRange.Model.WhenAnyValue(it => it.EndDate))
            .Select(_ => Unit.Default);
}