namespace SparkTrack.DataAccess.EFCore.Data;

using System.Linq.Expressions;
using Core.Shared.Data;
using Microsoft.EntityFrameworkCore;

public class PaginatedQueryable<TData>
{
    private readonly Func<Task<int>>   m_countTotalAsync;
    private readonly IQueryable<TData> m_source;
    private readonly PageQuery         m_pageQuery;

    public PaginatedQueryable(IQueryable<TData> source, PageQuery pageQuery)
    {
        m_source = source;
        m_pageQuery = pageQuery;
        m_countTotalAsync = () => source.CountAsync();
    }
    
    protected PaginatedQueryable(IQueryable<TData> source, Func<Task<int>> countTotalAsync, PageQuery pageQuery)
    {
        m_source = source;
        m_countTotalAsync = countTotalAsync;
        m_pageQuery = pageQuery;
    }

    public PaginatedQueryable<TData> Where(Expression<Func<TData, bool>> expression) =>
        new(m_source.Where(expression), m_countTotalAsync, m_pageQuery);
    
    public PaginatedQueryable<TData> WhereIf(bool condition, Expression<Func<TData, bool>> expression) => condition ?
        new(m_source.Where(expression), m_countTotalAsync, m_pageQuery) : this;

    public PaginatedQueryable<TResult> Select<TResult>(Expression<Func<TData, TResult>> expression) =>
        new(m_source.Select(expression), m_countTotalAsync, m_pageQuery);

    public async Task<IReadOnlyPagedData<TData>> CollectAsync()
    {
        var total = await m_countTotalAsync();

        var result = m_source;

        if (m_pageQuery is { Page: > 0, ItemsPerPage: > 0 } )
        {
            result = result.Skip((m_pageQuery.Page - 1) * m_pageQuery.ItemsPerPage).Take(m_pageQuery.ItemsPerPage);
        }

        return new ReadOnlyPagedData<TData>(await result.ToArrayAsync(), total);
    }
}