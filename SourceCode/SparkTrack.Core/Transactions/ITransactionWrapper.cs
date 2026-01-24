namespace SparkTrack.Core.Transactions;

public interface ITransactionWrapper
{
    Task ExecuteInTransactionAsync(Func<Task> action);
}