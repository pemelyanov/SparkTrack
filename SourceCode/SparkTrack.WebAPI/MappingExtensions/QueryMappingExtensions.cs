namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data;
using DTO;

public static class QueryMappingExtensions
{
    
    public static PageQuery ToDomain(this PageQueryDTO it) => new(it.Page, it.ItemsPerPage);
    
    public static SortQuery ToDomain(this SortQueryDTO it) => new(it.SortField, it.SortDescending);
}