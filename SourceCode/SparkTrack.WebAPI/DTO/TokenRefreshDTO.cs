namespace SparkTrack.WebAPI.DTO;

public record TokenRefreshDTO
{
    public required string RefreshToken { get; init; }
}   