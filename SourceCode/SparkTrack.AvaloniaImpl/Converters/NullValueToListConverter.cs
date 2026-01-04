namespace SparkTrack.AvaloniaImpl.Converters;

using Avalonia.Data.Converters;
using System.Collections;
using System.Globalization;

public class NullValueToListConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable enumerable) return null;

        return enumerable.Cast<object>().Prepend(null);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}