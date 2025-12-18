namespace SparkTrack.Authentication.DataAccess.EFCore.Data;

public class RefreshTokenData<TUserKey>
{
    public Guid Id { get; set; }
    public TUserKey UserId { get; set; } = default!;
    public string Token { get; set; } = null!;
    public string TokenHash { get; set; } = null!;
    public DateTime GenerationDate { get; set; } = DateTime.UtcNow;
}
