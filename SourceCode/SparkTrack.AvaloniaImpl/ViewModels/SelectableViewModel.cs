namespace SparkTrack.AvaloniaImpl.ViewModels;

using ReactiveUI.Fody.Helpers;

public class SelectableViewModel<TData>(TData model)
{
    public TData Model { get; } = model;
    
    [Reactive]
    public bool IsSelected { get; set; }
}