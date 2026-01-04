namespace SparkTrack.AvaloniaImpl.Converters;

using Avalonia.Data.Converters;
using System.Globalization;

public enum ECompareOperation
{
    Eq,
    Gt,
    Lt,
    Egt,
    Elt
}

public class ComparisonConverter : IValueConverter
{
    public ECompareOperation Operation { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || !double.TryParse(value.ToString(), out var doubleValue) || parameter is null
            || !double.TryParse(parameter.ToString(), out var doubleParameter)) return null;

        return Operation switch
        {
            ECompareOperation.Eq => Math.Abs(doubleValue - doubleParameter) < 0.0001,
            ECompareOperation.Elt => doubleValue <= doubleParameter,
            ECompareOperation.Egt => doubleValue >= doubleParameter,
            ECompareOperation.Gt => doubleValue > doubleParameter,
            ECompareOperation.Lt => doubleValue < doubleParameter,
            _ => null
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}