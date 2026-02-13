namespace SparkTrack.AvaloniaImpl.Extensions;

using System.Globalization;

public static class StringExtensions
{
    public static string ToPascalCase(this string source)
    {
        var yourString = source.ToLower().Replace("_", " ").Replace("-", " ");
        TextInfo info = CultureInfo.CurrentCulture.TextInfo;
        return info.ToTitleCase(yourString).Replace(" ", string.Empty);
    }
}