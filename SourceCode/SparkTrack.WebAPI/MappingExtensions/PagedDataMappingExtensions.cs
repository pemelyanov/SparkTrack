namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data;
using DTO;

public static class PagedDataMappingExtensions
{
    public static PagedDTO<TData> ToDTO<TSource, TData>(
        this IReadOnlyPagedData<TSource> source,
        Func<TSource, TData> converter
    ) => new()
    {
        Items = source.Items.Select(converter).ToArray(),
        Total = source.Total
    };
}