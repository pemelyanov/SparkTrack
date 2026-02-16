using Avalonia.ReactiveUI;
using ReactiveUI;

namespace SparkTrack.AvaloniaImpl.Pages.Settings;

[SingleInstanceView]
public partial class SettingsPage : ReactiveUserControl<SettingsPageViewModel>
{
    public SettingsPage()
    {
        InitializeComponent();
    }
}