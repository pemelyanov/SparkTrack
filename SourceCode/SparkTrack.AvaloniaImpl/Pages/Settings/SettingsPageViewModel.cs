using System.Reactive.Disposables;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkTrack.AvaloniaImpl.Data;
using SparkTrack.AvaloniaImpl.Services.Explorer;
using SparkTrack.Core.Client.Services.Configuration;

namespace SparkTrack.AvaloniaImpl.Pages.Settings;

public class SettingsPageViewModel(
    Lazy<IScreen> hostScreen,
    IExplorerService explorerService,
    IConfigurationService<InterfaceConfiguration> interfaceConfigurationService)
    : ViewModelBase, IRoutableViewModel
{
    protected override void OnFirstActivated(CompositeDisposable disposables)
    {
        base.OnFirstActivated(disposables);

        this.WhenAnyValue(it => it.Scale)
            .Subscribe(value => interfaceConfigurationService.UpdateConfig(interfaceConfigurationService.Config with
            {
                Scale = value
            }))
            .DisposeWith(disposables);
    }

    public string UrlPathSegment => "settings";

    public IScreen HostScreen => hostScreen.Value;

    [Reactive]
    public int Scale { get; set; } = interfaceConfigurationService.Config.Scale;

    public void OpenLogsFolder() => explorerService.OpenFolder(NLogConfigManager.LogsFolder);
}