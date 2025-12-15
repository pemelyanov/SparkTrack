namespace SparkTrack.AvaloniaImpl.Converters;

using Avalonia.Data.Converters;
using Pages.Features;
using Symbol = FluentAvalonia.UI.Controls.Symbol;

public static class NavigationViewItemConverters
{
    public static IValueConverter Symbol { get; } = new FuncValueConverter<Type, Symbol?>(
        type => type switch
        {
            _ when type == typeof(FeaturesPageViewModel) => FluentAvalonia.UI.Controls.Symbol.List,
            _ => null
        }
    );
    
    public static IValueConverter Name { get; } = new FuncValueConverter<Type, string?>(
        type => type switch
        {
            _ when type == typeof(FeaturesPageViewModel) => "Идеи",
            _ => null
        }
    );
}