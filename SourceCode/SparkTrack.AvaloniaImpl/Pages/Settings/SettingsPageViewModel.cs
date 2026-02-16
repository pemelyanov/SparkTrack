using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using SparkTrack.AvaloniaImpl.Services.Explorer;

namespace SparkTrack.AvaloniaImpl.Pages.Settings;

public class SettingsPageViewModel(Lazy<IScreen> hostScreen, IExplorerService explorerService) : ViewModelBase, IRoutableViewModel
{
    public string UrlPathSegment => "settings";

    public IScreen HostScreen => hostScreen.Value;

    public void OpenLogsFolder() => explorerService.OpenFolder(NLogConfigManager.LogsFolder);
}