namespace SparkTrack.DataAccess.EFCore;

using Data.Entities;
using Microsoft.EntityFrameworkCore;

public class SparkTrackDbContext(DbContextOptions<SparkTrackDbContext> options) : DbContext(options)
{
    public DbSet<UserData> Users => Set<UserData>();
    public DbSet<ProjectData> Projects  => Set<ProjectData>();
    public DbSet<FeatureData> Features  => Set<FeatureData>();
    public DbSet<SubTaskData> SubTasks  => Set<SubTaskData>();
    public DbSet<FileData> Files  => Set<FileData>();
}