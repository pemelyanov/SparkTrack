namespace SparkTrack.DataAccess.EFCore.Configuration;

using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProjectDataConfiguration : IEntityTypeConfiguration<ProjectData>
{
    public virtual void Configure(EntityTypeBuilder<ProjectData> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(p => p.Link)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.HasMany(p => p.Features)
            .WithOne(p => p.Project);
        
        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.Property(p => p.ArchivedAt)
            .IsRequired(false);
        
        builder.Property(p => p.ArchiveSource)
            .IsRequired(false);
    }
}