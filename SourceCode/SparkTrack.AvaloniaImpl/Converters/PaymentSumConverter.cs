namespace SparkTrack.AvaloniaImpl.Converters;

using System.Globalization;
using Avalonia.Data.Converters;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;

public class PaymentSumConverter : IMultiValueConverter, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<PaymentInfo> payments) return null;

        var paymentType = parameter as EPaymentType?;

        return payments.Where(it => paymentType is null || it.PaymentType == paymentType).Sum(it => it.Payment);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is not IEnumerable<PaymentInfo> payments || values[1] is not EPaymentType paymentType) return null;

        return Convert(payments, targetType, paymentType, culture);
    }
}