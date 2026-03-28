namespace SparkTrack.DeepLink.Data;

public abstract record PageData
{
    public sealed record Feature(int Id) : PageData;
}