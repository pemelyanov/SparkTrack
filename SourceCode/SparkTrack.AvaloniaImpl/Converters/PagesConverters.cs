namespace SparkTrack.AvaloniaImpl.Converters;

using Avalonia.Data.Converters;
using Pages.Features;
using Pages.Users;
using Symbol = FluentAvalonia.UI.Controls.Symbol;

// TODO: Это кринж, надо сделать поудобней
public static class PagesConverters
{
    public static IValueConverter Symbol { get; } = new FuncValueConverter<Type, Symbol?>(
        type => type switch
        {
            _ when type == typeof(FeaturesPageViewModel) => FluentAvalonia.UI.Controls.Symbol.List,
            _ when type == typeof(UsersPageViewModel) => FluentAvalonia.UI.Controls.Symbol.OtherUser,
            _ => null
        }
    );
    
    public static IValueConverter Name { get; } = new FuncValueConverter<Type, string?>(
        type => type switch
        {
            _ when type == typeof(FeaturesPageViewModel) => "Идеи",
            _ when type == typeof(UsersPageViewModel) => "Пользователи",
            _ => null
        }
    );
}