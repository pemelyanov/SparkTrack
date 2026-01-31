namespace SparkTrack.AvaloniaImpl.Converters;

using System.Globalization;
using Avalonia.Data.Converters;
using Core.Shared.Enums;

public class PaymentTypeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not EPaymentType type) return null;

        return type switch
        {
            EPaymentType.Main => "Основной оклад",
            EPaymentType.TimelyBonus => "Бонус за срок",
            _ => throw new NotSupportedException()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}