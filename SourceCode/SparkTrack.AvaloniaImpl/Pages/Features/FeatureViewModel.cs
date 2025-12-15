namespace SparkTrack.AvaloniaImpl.Pages.Features;

using Core.Shared.Data.Entities;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI.Fody.Helpers;

public class FeatureViewModel(Feature model) : ViewModelBase
{
    public Feature Model { get; } = model;
    
    [Reactive]
    public bool IsSelected { get; set; }
}