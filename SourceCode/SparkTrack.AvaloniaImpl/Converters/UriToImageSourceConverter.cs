namespace SparkTrack.AvaloniaImpl.Converters;

using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System.Globalization;

public class UriToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string uri || string.IsNullOrEmpty(uri)) return null;

        var base64Mark = "base64:";
        if (uri.StartsWith(base64Mark))
        {
            var base64 = uri.Substring(base64Mark.Length);
            var data = System.Convert.FromBase64String(base64);
            using var memoryStream = new MemoryStream(data);
            return new Bitmap(memoryStream);
        }

        if (!File.Exists(uri)) return null;

        return new Bitmap(uri);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}