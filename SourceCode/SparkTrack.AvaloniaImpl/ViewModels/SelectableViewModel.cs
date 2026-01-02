namespace SparkTrack.AvaloniaImpl.ViewModels;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

public class SelectableViewModel<TData>(TData model) : ReactiveObject
{
    public TData Model { get; } = model;

    [Reactive]
    public bool IsSelected { get; set; }
}