namespace SparkTrack.DataAccess.EFCore.Extensions;

using System.Linq.Expressions;
using Core.Shared.Data;
using Data;

public static class QueryableExtensions
{
    public static IQueryable<TData> WhereIf<TData>(this IQueryable<TData> queryable, bool condition, Expression<Func<TData, bool>> expression)
    {
        if (!condition) return queryable;

        return queryable.Where(expression);
    }

    public static PaginatedQueryable<TData> AsPaginated<TData>(this IQueryable<TData> queryable, PageQuery query) =>
        new(queryable, query);
}