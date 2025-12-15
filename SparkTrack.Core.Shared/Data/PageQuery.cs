namespace SparkTrack.Core.Shared.Data;

public record PageQuery(int Page, int ItemsPerPage)
{
    public PageQuery All => new(-1, -1);
}