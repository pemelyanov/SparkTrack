namespace SparkTrack.DataAccess.EFCore;

using Authentication.DataAccess.EFCore;
using Configuration;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

public class SparkTrackDbContext(
    DbContextOptions<SparkTrackDbContext> options,
    IConfigurationApplier? configurationApplier = null
) : RefreshTokenDbContext<Guid>(options)
{
    public DbSet<UserData> Users => Set<UserData>();

    public DbSet<ProjectData> Projects => Set<ProjectData>();

    public DbSet<FeatureData> Features => Set<FeatureData>();

    public DbSet<SubTaskData> SubTasks => Set<SubTaskData>();

    public DbSet<AttachmentData> Attachments => Set<AttachmentData>();
    
    public DbSet<CommentData> Comments => Set<CommentData>();
    
    public DbSet<PaymentData> Payments => Set<PaymentData>();
    
    public DbSet<BonusPaymentData> Bonuses => Set<BonusPaymentData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (configurationApplier is null)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SparkTrackDbContext).Assembly);
        else configurationApplier.ApplyConfiguration(modelBuilder);
    }
}