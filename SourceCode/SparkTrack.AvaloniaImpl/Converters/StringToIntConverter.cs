namespace SparkTrack.AvaloniaImpl.Converters;

using System;
using System.Globalization;

public class StringToIntConverter : ConverterBase<StringToIntConverter>
{
    public override object? Convert(object? value, Type targetType = null!, object? parameter = null, CultureInfo culture = null!)
    {
        if (value is not int number) return null;

        return number.ToString();
    }

    public override object? ConvertBack(object? value, Type targetType = null!, object? parameter = null, CultureInfo culture = null!)
    {
        if (value is null) return 0;
        
        if (value is not string numberText) return 0;

        return int.TryParse(numberText, out var number) ? number : 0;
    }
}