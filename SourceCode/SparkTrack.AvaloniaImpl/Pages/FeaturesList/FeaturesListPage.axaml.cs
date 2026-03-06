using Avalonia.Interactivity;
using SparkTrack.AvaloniaImpl.Data.Configurations;
using SparkTrack.Core.Client.Services.Configuration;
using Splat;

namespace SparkTrack.AvaloniaImpl.Pages.FeaturesList;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using ViewModels;
using Core.Shared.Data.Entities;
using Extensions;
using ReactiveUI;

[SingleInstanceView]
public partial class FeaturesListPage : ReactiveUserControl<FeaturesListPageViewModel>
{
    private readonly IConfigurationService<FeaturesPageConfig> m_pageConfig =
        Locator.Current.GetService<IConfigurationService<FeaturesPageConfig>>()!;
    
    public FeaturesListPage()
    {
        InitializeComponent();
        
        FeaturesTable.InitializeColumnsWidth(m_pageConfig.Config.ColumnWidths);
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

    private void FeaturesTable_OnUnloaded(object? sender, RoutedEventArgs e)
    {
        FeaturesTable.SaveColumnsWidth(m_pageConfig);
    }
}