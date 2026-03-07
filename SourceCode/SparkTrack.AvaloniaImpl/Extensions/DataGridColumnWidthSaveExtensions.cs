namespace SparkTrack.AvaloniaImpl.Extensions;

using Avalonia.Controls;
using Core.Client.Extensions;
using Core.Client.Services.Configuration;
using Data.Configurations;

public static class DataGridColumnWidthSaveExtensions
{
    public static void SaveColumnsWidth<TConfig>(
        this DataGrid dataGrid,
        IConfigurationService<TConfig> configurationService
    )
        where TConfig : struct, IColumnsConfig
    {
        var columns = new Dictionary<string, double>();
        foreach (var column in dataGrid.Columns)
        {
            if (column.Width.IsStar || column.Tag is not string name) continue;

            columns[name] = column.ActualWidth;
        }

        configurationService.Update(it => it with
            {
                ColumnWidths = columns
            }
        );
    }

    public static void InitializeColumnsWidth(this DataGrid dataGrid, IDictionary<string, double> columns)
    {
        foreach ((var name, var width) in columns)
        {
            var existingColumn = dataGrid.Columns.FirstOrDefault(it => (string)it.Tag == name);

            if (existingColumn is null)
                continue;

            if (existingColumn.Width.IsStar)
                continue;

            existingColumn.Width = new DataGridLength(width);
        }
    }
}