namespace SparkTrack.AvaloniaImpl.Converters;

using System.Globalization;
using Avalonia.Data.Converters;
using Core.Shared.Enums;

public class RoleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ERole role) return null;

        return role switch
        {
            ERole.Employee => "Сотрудник",
            ERole.Admin => "Администратор",
            ERole.God => "Боженька",
            _ => throw new NotSupportedException()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}