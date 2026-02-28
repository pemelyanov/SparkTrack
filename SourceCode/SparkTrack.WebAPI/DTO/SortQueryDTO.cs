namespace SparkTrack.WebAPI.DTO;

public record SortQueryDTO
{
    public string SortField { get; init; } = string.Empty;
    
    public bool SortDescending { get; init; }
}