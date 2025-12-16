namespace SparkTrack.WebAPI.DTO;

public record PagedDTO<TData>
{
    public IReadOnlyList<TData> Items { get; init; } = [];
    
    public long Total { get; init; }
}