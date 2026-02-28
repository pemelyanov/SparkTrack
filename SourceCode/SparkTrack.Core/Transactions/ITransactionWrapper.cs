namespace SparkTrack.Core.Transactions;

public interface ITransactionWrapper
{
    Task ExecuteInTransactionAsync(Func<Task> action);
    
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
}