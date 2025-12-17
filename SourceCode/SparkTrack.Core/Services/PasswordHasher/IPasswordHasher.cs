namespace SparkTrack.Core.Services.PasswordHasher;

public interface IPasswordHasher
{
    Task<string> HashAsync(string password);

    Task<bool> VerifyAsync(string password, string hash);
}