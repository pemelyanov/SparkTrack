namespace SparkTrack.DataAccess.EFCore.Extensions;

using System.Linq.Expressions;
using Core.Shared.Data;
using Data;
using NLog;

public static class QueryableExtensions
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    
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

        try
        {
            var expression = expressionSelector();

            if (query.SortDescending) return queryable.OrderByDescending(expression);

            return queryable.OrderBy(expression);
        }
        catch (NotSupportedException e)
        {
            s_logger.Warn(e, "Unsupported sort field");
            return queryable;
        }
    }

    public static PaginatedQueryable<TData> AsPaginated<TData>(this IQueryable<TData> queryable, PageQuery query) =>
        new(queryable, query);
}