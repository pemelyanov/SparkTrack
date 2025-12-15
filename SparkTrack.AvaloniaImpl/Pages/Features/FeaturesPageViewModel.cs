namespace SparkTrack.AvaloniaImpl.Pages.Features;

using Fanatiki.MVVM.ViewModels;
using ReactiveUI;

public class FeaturesPageViewModel(Lazy<IScreen> screen) : ViewModelBase, IRoutableViewModel
{
    public string? UrlPathSegment => "features";

    public IScreen HostScreen => screen.Value;
}