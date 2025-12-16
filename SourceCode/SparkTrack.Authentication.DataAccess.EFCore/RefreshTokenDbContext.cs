namespace SparkTrack.Authentication.DataAccess.EFCore;

using Data;
using Microsoft.EntityFrameworkCore;

public class RefreshTokenDbContext<TUserId> : DbContext
{
    public RefreshTokenDbContext()
    {
    }

    public RefreshTokenDbContext(DbContextOptions<RefreshTokenDbContext<TUserId>> options)
        : base(options)
    {
    }

    public RefreshTokenDbContext(DbContextOptions options)
        : base(options)
    {
    }
    
    public DbSet<RefreshTokenData<TUserId>> RefreshTokens => Set<RefreshTokenData<TUserId>>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}