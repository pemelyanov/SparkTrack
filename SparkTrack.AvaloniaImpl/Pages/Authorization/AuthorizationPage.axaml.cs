using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SparkTrack.AvaloniaImpl.Pages.Authorization;

using Avalonia.ReactiveUI;

public partial class AuthorizationPage : ReactiveUserControl<AuthorizationPageViewModel>
{
    public AuthorizationPage()
    {
        InitializeComponent();
    }
}