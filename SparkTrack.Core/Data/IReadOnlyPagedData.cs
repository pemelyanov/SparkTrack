namespace SparkTrack.Core.Data;

public interface IReadOnlyPagedData<out TData>
{
    IReadOnlyList<TData> Items { get; }
    
    long Total { get; }
}