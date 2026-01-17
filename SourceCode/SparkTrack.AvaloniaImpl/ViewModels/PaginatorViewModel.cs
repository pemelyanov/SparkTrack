namespace SparkTrack.AvaloniaImpl.ViewModels;

using Core.Shared.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;

public class PaginatorViewModel : ReactiveObject
{
    [Reactive]
    public int PagesQuantity { get; set; }

    [Reactive]
    public int CurrentPage { get; set; }

    [Reactive]
    public int ItemsPerPage { get; set; }

    public PageQuery ToQuery() => new(CurrentPage, ItemsPerPage);

    public void SetPagesQuantity(long totalItems)
    {
        var pages = totalItems / (float)ItemsPerPage;

        PagesQuantity = (int)Math.Ceiling(pages);
    }
    
    public IObservable<PaginatorViewModel> WhenChanged() => this
        .WhenAnyValue(it => it.CurrentPage, it => it.ItemsPerPage)
        .Select(_ => this);
}