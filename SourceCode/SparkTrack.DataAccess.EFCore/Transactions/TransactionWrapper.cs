namespace SparkTrack.DataAccess.EFCore.Transactions;

using Core.Transactions;

public class TransactionWrapper(SparkTrackDbContext dbContext) : ITransactionWrapper
{
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            await action();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}