namespace SparkTrack.AvaloniaImpl.Windows.Main;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Services.DialogHost;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>, IDialogHost
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private void NavigationItem_OnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Control { Tag: Type type }) return;

        ViewModel?.SelectPage(type);
    }

    public Task ShowAsync(ReactiveObject viewModel)
    {
        var view = ViewLocator.Current.ResolveView(viewModel);

        if (view != null) view.ViewModel = viewModel;

        return view switch
        {
            ContentDialog contentDialog => contentDialog.ShowAsync(this),
            _ => throw new NotSupportedException()
        };
    }
}