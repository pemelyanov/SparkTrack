namespace SparkTrack.WebAPI.DTO;

public record FeatureFilterQueryDTO
{
    public Guid? ProjectId { get; init; } = null;

    public bool ShowClosed { get; init; } = false;

    public bool ShowCompleted { get; init; } = true;

    public DateTime? StartDate { get; init; } = null;

    public DateTime? EndDate { get; init; } = null;
}