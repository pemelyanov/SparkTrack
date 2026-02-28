namespace SparkTrack.AvaloniaImpl.Converters;

using Avalonia.Data.Converters;
using Core.Shared.Data.Entities;
using System.Globalization;
using Core.Client.Services.Authorization;
using Core.Shared.Enums;
using Core.Shared.Extensions;
using Splat;

public class FeatureDeadlineConverter : IValueConverter
{
    private IAuthorizationService m_authorizationService = Locator.Current.GetService<IAuthorizationService>()!;
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Feature feature) return null;

        if (feature.TasksList.Count == 0) return null;

        var currentUser = m_authorizationService.CurrentUser.Value!; 
        if(currentUser.Role.IsAnyRole(ERole.Admin))
            return feature.TasksList.Min(it => it.Deadline);

        return feature.TasksList.Where(it => it.ExecutorEmployee.Id == currentUser.Id).Min(it => it.Deadline);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}