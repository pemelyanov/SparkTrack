namespace SparkTrack.AvaloniaImpl.Converters;

using System;
using System.Globalization;
using Avalonia.Data.Converters;

public abstract class ConverterBase<TConverter> : IValueConverter
    where TConverter : ConverterBase<TConverter>, new()
{
    public static TConverter Instance { get; } = new();

    public abstract object? Convert(object? value, Type targetType = null!, object? parameter = null,
        CultureInfo culture = null!);

    public virtual object? ConvertBack(object? value, Type targetType = null!, object? parameter = null,
        CultureInfo culture = null!)
    {
        return null;
    }
}