using Avalonia.Interactivity;
using NLog;
using SparkTrack.AvaloniaImpl.Data.Configurations;
using SparkTrack.Core.Client.Extensions;
using SparkTrack.Core.Client.Services.Configuration;
using Splat;
using ILogger = NLog.ILogger;

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
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    
    private readonly IConfigurationService<FeaturesPageConfig> m_featuresPageConfig =
        Locator.Current.GetService<IConfigurationService<FeaturesPageConfig>>()!;
    
    public FeaturesListPage()
    {
        InitializeComponent();

        if (m_featuresPageConfig.Config.ColumnWidths is { } columns)
        {
            s_logger.Info("Initializing saved columns widths...");
            foreach ((var name, var width) in columns)
            {
                var existingColumn = FeaturesTable.Columns.FirstOrDefault(it => (string)it.Tag ==name);

                if (existingColumn is null)
                {
                    s_logger.Warn("Cannot find column with name {name}", name);
                    continue;
                }

                if (existingColumn.Width.IsStar)
                {
                    s_logger.Debug("{name} width is star, skipping", name);
                    continue;
                }

                existingColumn.Width = new DataGridLength(width);
            }
        }
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
        var columns = new Dictionary<string, double>();
        foreach (var column in FeaturesTable.Columns)
        {
            if(column.Width.IsStar || column.Tag is not string name) continue;

            columns[name] = column.ActualWidth;
        }
        
        m_featuresPageConfig.Update(it => it with
        {
            ColumnWidths = columns
        });
    }
}