namespace SparkTrack.AvaloniaImpl.Pages.FeaturesList;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using ViewModels;
using Core.Shared.Data.Entities;
using ReactiveUI;

[SingleInstanceView]
public partial class FeaturesListPage : ReactiveUserControl<FeaturesListPageViewModel>
{
    public FeaturesListPage()
    {
        InitializeComponent();
    }

    private void DataGrid_OnSorting(object? sender, DataGridColumnEventArgs e)
    {
        e.Handled = true;
    }

    private void DataGrid_OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.DoubleTapped += RowOnDoubleTapped;
    }
    
    private void DataGrid_OnUnloadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.DoubleTapped -= RowOnDoubleTapped;
    }

    private void RowOnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if(sender is not Control { DataContext: SelectableViewModel<Feature> featureViewModel }) return;
        
        ViewModel?.OpenFeature(featureViewModel.Model);
    }
}