namespace SparkTrack.Authentication.Core.Models;

public class JwtConfiguration
{
    public string AccessTokenSecret { get; set; } = null!;
    public string RefreshTokenSecret { get; set; } = null!;
    public double AccessTokenExpirationMinutes { get; set; }
    public double RefreshTokenExpirationMinutes { get; set; }
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
}