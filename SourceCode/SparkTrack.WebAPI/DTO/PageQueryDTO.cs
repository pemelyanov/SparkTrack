namespace SparkTrack.WebAPI.DTO;

public record PageQueryDTO
{
    public int Page { get; init; }
    
    public int ItemsPerPage { get; init; }
}