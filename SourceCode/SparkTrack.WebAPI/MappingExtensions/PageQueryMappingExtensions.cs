namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data;
using DTO;

public static class PageQueryMappingExtensions
{
    public static PageQueryDTO ToDTO(this PageQuery it) => new()
    {
        Page = it.Page,
        ItemsPerPage = it.ItemsPerPage
    };

    public static PageQuery ToDomain(this PageQueryDTO it) => new(it.Page, it.ItemsPerPage);
}