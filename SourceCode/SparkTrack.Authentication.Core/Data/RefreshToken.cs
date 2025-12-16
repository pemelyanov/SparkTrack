namespace SparkTrack.Authentication.Core.Data;

public class RefreshToken<TUserKey>
{
    public Guid Id { get; set; }
    public TUserKey UserId { get; set; } = default!;
    public string Token { get; set; } = null!;
    public string TokenHash { get; set; } = null!;
    public DateTime GenerationDate { get; set; } = DateTime.Now;
}