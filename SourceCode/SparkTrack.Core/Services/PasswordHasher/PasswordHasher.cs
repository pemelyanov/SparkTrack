namespace SparkTrack.Core.Services.PasswordHasher;

internal class PasswordHasher : IPasswordHasher
{
    public Task<string> HashAsync(string password)
    {
        return Task.Run(() => BCrypt.Net.BCrypt.HashPassword(password));
    }

    public Task<bool> VerifyAsync(string password, string hash)
    {
        return Task.Run(() => BCrypt.Net.BCrypt.Verify(password, hash));
    }
}