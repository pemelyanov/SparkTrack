namespace SparkTrack.AvaloniaImpl.Converters;

using Avalonia.Data.Converters;
using Core.Shared.Data.Entities;
using System.Globalization;

public class FeatureDeadlineConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Feature feature) return null;

        if (feature.TasksList.Count == 0) return null;

        return feature.TasksList.Min(it => it.Deadline);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}