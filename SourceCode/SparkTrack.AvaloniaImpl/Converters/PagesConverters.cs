using SparkTrack.AvaloniaImpl.Pages.Settings;

namespace SparkTrack.AvaloniaImpl.Converters;

using Avalonia.Data.Converters;
using Pages.AdminFinance;
using Pages.FeaturesList;
using Pages.ProjectsList;
using Pages.UsersList;

// TODO: Это кринж, надо сделать поудобней
public static class PagesConverters
{
    public static IValueConverter Symbol { get; } = new FuncValueConverter<Type, FluentIcons.Common.Symbol?>(
        type => type switch
        {
            _ when type == typeof(FeaturesListPageViewModel) => FluentIcons.Common.Symbol.LightbulbFilament,
            _ when type == typeof(UsersListPageViewModel) => FluentIcons.Common.Symbol.PersonSettings,
            _ when type == typeof(ProjectsListPageViewModel) => FluentIcons.Common.Symbol.Production,
            _ when type == typeof(AdminFinancePageViewModel) => FluentIcons.Common.Symbol.CurrencyDollarEuro,
            _ when type == typeof(SettingsPageViewModel) => FluentIcons.Common.Symbol.Settings,
            _ => null
        }
    );
    
    public static IValueConverter Name { get; } = new FuncValueConverter<Type, string?>(
        type => type switch
        {
            _ when type == typeof(FeaturesListPageViewModel) => "Идеи",
            _ when type == typeof(UsersListPageViewModel) => "Пользователи",
            _ when type == typeof(ProjectsListPageViewModel) => "Проекты",
            _ when type == typeof(AdminFinancePageViewModel) => "Финансы",
            _ when type == typeof(SettingsPageViewModel) => "Настройки",
            _ => null
        }
    );
}