using Avalonia.Data.Converters;
using System.Globalization;

namespace SparkTrack.AvaloniaImpl.Converters;

public sealed class BytesToStringConverter : IValueConverter, IMultiValueConverter
{
    private static readonly string[] s_units =
    {
        "б", "кб", "мб", "гб"
    };

    public string Separator { get; set; } = " / ";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not long bytes || bytes < 0)
            return "0 б";

        int unitIndex = ResolveIndex(bytes);

        return Format(unitIndex, bytes);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 1 || values[^1] is not long lastBytes) return null;

        var unitIndex = ResolveIndex(lastBytes);;

        return string.Join(Separator, values.OfType<long>().Select(it => Format(unitIndex, it, false))) +
               $" {s_units[unitIndex]}";
    }

    private static int ResolveIndex(long bytes)
    {
        double size = bytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < s_units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return unitIndex;
    }

    private static double ResolveSize(int unitIndex, long bytes)
    {
        return unitIndex > 0 ? bytes / Math.Pow(1024, unitIndex) : bytes;
    }

    private static object Format(int unitIndex, long bytes, bool withUnits = true)
    {
        double size = ResolveSize(unitIndex, bytes);
        var unitsString = withUnits ? $" {s_units[unitIndex]}" : string.Empty;

        return unitIndex == 0
            ? $"{size}{unitsString}"
            : $"{size:0.00}{unitsString}";
    }
}