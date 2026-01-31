namespace SparkTrack.Core.Shared.Extensions;

using System.Collections;
using System.Reflection;
using Data;

public static class PagedDataExtensions
{
    public static IReadOnlyPagedData<TResult> Convert<TSource, TResult>(
        this IReadOnlyPagedData<TSource> source,
        Func<TSource, TResult> selector
    ) => new ReadOnlyPagedData<TResult>(source.Items.Select(selector).ToArray(), source.Total);

    public static IReadOnlyPagedData<TResult> ReflectionConvert<TSource, TResult>(
        this object source,
        Func<TSource, TResult> selector
    )
    {
        var type = source.GetType();

        var itemsProp = type.GetProperty("Items", BindingFlags.Public | BindingFlags.Instance);
        var totalProp = type.GetProperty("Total", BindingFlags.Public | BindingFlags.Instance);

        if (itemsProp == null || totalProp == null)
            throw new InvalidOperationException(
                $"Тип {type.Name} должен содержать свойства Items и Total"
            );

        var itemsValue = itemsProp.GetValue(source);
        var totalValue = totalProp.GetValue(source);

        if (itemsValue is not IEnumerable enumerable)
            throw new InvalidOperationException("Items не является IEnumerable");

        var items = enumerable
            .Cast<TSource>()
            .Select(selector)
            .ToArray();

        var total = (long)totalValue!;

        return new ReadOnlyPagedData<TResult>(items, total);
    }
}