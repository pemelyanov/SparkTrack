namespace SparkTrack.Core.Data;

public record PageQuery(int Page, int ItemsPerPage)
{
    public PageQuery All => new(-1, -1);
}