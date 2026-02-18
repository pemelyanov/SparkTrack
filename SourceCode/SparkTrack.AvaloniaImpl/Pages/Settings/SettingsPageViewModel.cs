using System.Reactive.Disposables;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkTrack.AvaloniaImpl.Data;
using SparkTrack.AvaloniaImpl.Data.Configurations;
using SparkTrack.AvaloniaImpl.Services.Explorer;
using SparkTrack.Core.Client.Services.Configuration;

namespace SparkTrack.AvaloniaImpl.Pages.Settings;

using Core.Client.Extensions;

public class SettingsPageViewModel : ViewModelBase, IRoutableViewModel
{
    private          InterfaceConfiguration                        m_initialInterfaceConfiguration;
    private readonly Lazy<IScreen>                                 m_hostScreen;
    private readonly IExplorerService                              m_explorerService;
    private readonly IConfigurationService<InterfaceConfiguration> m_interfaceConfigurationService;

    public SettingsPageViewModel(Lazy<IScreen> hostScreen,
                                 IExplorerService explorerService,
                                 IConfigurationService<InterfaceConfiguration> interfaceConfigurationService)
    {
        m_hostScreen = hostScreen;
        m_explorerService = explorerService;
        m_interfaceConfigurationService = interfaceConfigurationService;
        m_initialInterfaceConfiguration = interfaceConfigurationService.Config;
        Scale = interfaceConfigurationService.Config.Scale;
        m_interfaceConfigurationService.ConfigChanged += InterfaceConfigurationService_OnConfigChanged;
    }

    protected override void OnFirstActivated(CompositeDisposable disposables)
    {
        base.OnFirstActivated(disposables);

        this.WhenAnyValue(it => it.Scale)
            .Subscribe(value => m_interfaceConfigurationService.Update(it => it with
            {
                Scale = value
            }))
            .DisposeWith(disposables);
    }

    public string UrlPathSegment => "settings";

    public IScreen HostScreen => m_hostScreen.Value;
    
    [Reactive]
    public bool RestartNeeded { get; private set; }

    [Reactive]
    public int Scale { get; set; }

    public void OpenLogsFolder() => m_explorerService.OpenFolder(NLogConfigManager.LogsFolder);
    
    private void InterfaceConfigurationService_OnConfigChanged(InterfaceConfiguration config)
    {
        RestartNeeded = m_initialInterfaceConfiguration.Scale != config.Scale;
    }
}