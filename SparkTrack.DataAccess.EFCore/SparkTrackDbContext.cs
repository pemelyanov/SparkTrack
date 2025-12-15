namespace SparkTrack.DataAccess.EFCore;

using Data.Entities;
using Microsoft.EntityFrameworkCore;

public class SparkTrackDbContext : DbContext
{
    public DbSet<UserData> Users { get; } = null!;
    public DbSet<ProjectData> Projects { get; } = null!;
    public DbSet<FeatureData> Features { get; } = null!;
    public DbSet<SubTaskData> SubTasks { get; } = null!;
    public DbSet<FileData> Files { get; } = null!;
}