namespace SparkTrack.DataAccess.EFCore.Extensions;

using System.Linq.Expressions;
using Core.Shared.Data;
using Data;

public static class QueryableExtensions
{
    public static IQueryable<TData> WhereIf<TData>(
        this IQueryable<TData> queryable,
        bool condition,
        Expression<Func<TData, bool>> expression
    )
    {
        if (!condition) return queryable;

        return queryable.Where(expression);
    }

    public static IQueryable<TData> OrderBy<TData>(
        this IQueryable<TData> queryable,
        SortQuery? query,
        Func<Expression<Func<TData, object>>> expressionSelector
    )
    {
        if (query is null || string.IsNullOrEmpty(query.SortField)) return queryable;

        if (query.SortDescending) return queryable.OrderByDescending(expressionSelector());

        return queryable.OrderBy(expressionSelector());
    }

    public static PaginatedQueryable<TData> AsPaginated<TData>(this IQueryable<TData> queryable, PageQuery query) =>
        new(queryable, query);
}