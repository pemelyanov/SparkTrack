using Avalonia.Data.Converters;
using System.Globalization;

namespace SparkTrack.AvaloniaImpl.Converters;

public sealed class BytesToStringConverter : IValueConverter
{
    private static readonly string[] s_units =
    {
        "б", "кб", "мб", "гб", "тб", "пб"
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not long bytes || bytes < 0)
            return "0 б";

        double size = bytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < s_units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return unitIndex == 0
            ? $"{bytes} {s_units[unitIndex]}"
            : $"{size:0.##} {s_units[unitIndex]}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}