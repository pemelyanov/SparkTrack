using Avalonia.Controls;

namespace SparkTrack.AvaloniaImpl.Pages.Features;

using Avalonia.ReactiveUI;

public partial class FeaturesPage : ReactiveUserControl<FeaturesPageViewModel>
{
    public FeaturesPage()
    {
        InitializeComponent();
    }

    private void DataGrid_OnSorting(object? sender, DataGridColumnEventArgs e)
    {
        e.Handled = true;
    }
}