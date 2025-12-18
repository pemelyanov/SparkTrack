namespace SparkTrack.API.Data;

public record TokensConfiguration
{
    public string AccessToken { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;
}