namespace SparkTrack.AvaloniaImpl.ViewModels;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

public interface ISelectable
{
    bool IsSelected { get; set; }
}

public class SelectableViewModel<TData>(TData model) : ReactiveObject, ISelectable
{
    public TData Model { get; } = model;

    [Reactive]
    public bool IsSelected { get; set; }
}