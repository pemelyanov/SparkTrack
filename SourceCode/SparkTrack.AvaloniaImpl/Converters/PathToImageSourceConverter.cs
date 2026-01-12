namespace SparkTrack.AvaloniaImpl.Converters;

using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System.Globalization;

public class PathToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not string path ? null : new Bitmap(path);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}