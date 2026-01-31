namespace SparkTrack.AvaloniaImpl.ViewModels;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

public class DateRangeViewModel : ReactiveObject
{
    [Reactive]
    public DateTime? StartDate { get; set; }

    [Reactive]
    public DateTime? EndDate { get; set; }
}