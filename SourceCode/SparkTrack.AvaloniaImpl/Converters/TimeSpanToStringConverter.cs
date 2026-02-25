namespace SparkTrack.AvaloniaImpl.Converters;

using System.Globalization;
using Avalonia.Data.Converters;

public class TimeSpanToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimeSpan time) return null;
        
        if (time == TimeSpan.Zero)
            return "0с";

        int days = time.Days;
        int hours = time.Hours;
        int minutes = time.Minutes;
        int seconds = time.Seconds;

        if (days > 0)
            return hours > 0 ? $"{days}д {hours}ч" : $"{days}д";

        if (hours > 0)
            return minutes > 0 ? $"{hours}ч {minutes}м" : $"{hours}ч";

        if (minutes > 0)
            return seconds > 0 ? $"{minutes}м {seconds}с" : $"{minutes}м";

        return $"{seconds}с";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}