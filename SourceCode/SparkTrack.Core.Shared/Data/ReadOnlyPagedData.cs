namespace SparkTrack.Core.Shared.Data;

public class ReadOnlyPagedData<TData>(IReadOnlyList<TData> items, long total) : IReadOnlyPagedData<TData>
{
    public IReadOnlyList<TData> Items { get; } = items;

    public long Total { get; } = total;
}