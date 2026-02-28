namespace SparkTrack.DataAccess.EFCore.Transactions;

using Core.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

public class TransactionWrapper(SparkTrackDbContext dbContext) : ITransactionWrapper
{
    private          IDbContextTransaction? m_activeTransaction;
    private readonly List<string>           m_activeSubTransactions = [];
    
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        if (m_activeTransaction is null) m_activeTransaction = await dbContext.Database.BeginTransactionAsync();

        var currentSubTransactionId = Guid.CreateVersion7().ToString();
        m_activeSubTransactions.Add(currentSubTransactionId);

        try
        {
            await m_activeTransaction.CreateSavepointAsync(currentSubTransactionId);
            await action();
            
            if(m_activeSubTransactions.Count == 1) 
                await m_activeTransaction.CommitAsync();
        }
        catch
        {
            await m_activeTransaction.RollbackToSavepointAsync(currentSubTransactionId);
            throw;
        }
        finally
        {
            m_activeSubTransactions.Remove(currentSubTransactionId);

            if (m_activeSubTransactions.Count == 0)
            {
                await m_activeTransaction.DisposeAsync();
                m_activeTransaction = null;
            }
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
    {
        if (m_activeTransaction is null) m_activeTransaction = await dbContext.Database.BeginTransactionAsync();

        var currentSubTransactionId = Guid.CreateVersion7().ToString();
        m_activeSubTransactions.Add(currentSubTransactionId);

        try
        {
            await m_activeTransaction.CreateSavepointAsync(currentSubTransactionId);
            var result = await action();
            
            if(m_activeSubTransactions.Count == 1) 
                await m_activeTransaction.CommitAsync();

            return result;
        }
        catch
        {
            await m_activeTransaction.RollbackToSavepointAsync(currentSubTransactionId);
            throw;
        }
        finally
        {
            m_activeSubTransactions.Remove(currentSubTransactionId);

            if (m_activeSubTransactions.Count == 0)
            {
                await m_activeTransaction.DisposeAsync();
                m_activeTransaction = null;
            }
        }
    }
}