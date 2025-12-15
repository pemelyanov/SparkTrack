namespace SparkTrack.DataAccess.EFCore;

using Configuration;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

public class SparkTrackDbContext(DbContextOptions<SparkTrackDbContext> options, IConfigurationApplier? configurationApplier = null) : DbContext(options)
{
    public DbSet<UserData> Users => Set<UserData>();
    public DbSet<ProjectData> Projects  => Set<ProjectData>();
    public DbSet<FeatureData> Features  => Set<FeatureData>();
    public DbSet<SubTaskData> SubTasks  => Set<SubTaskData>();
    public DbSet<FileData> Files  => Set<FileData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (configurationApplier is null)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SparkTrackDbContext).Assembly);
        else configurationApplier.ApplyConfiguration(modelBuilder);
    }
}