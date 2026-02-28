namespace SparkTrack.DataAccess.EFCore.Configuration;

using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FeatureDataConfiguration : IEntityTypeConfiguration<FeatureData>
{
    public virtual void Configure(EntityTypeBuilder<FeatureData> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(f => f.ProjectId)
            .IsRequired();
        
        builder.Property(f => f.Description)
            .IsRequired(false);
        
        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(f => f.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.TasksList)
            .WithOne(it => it.Feature)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(f => f.AttachmentsList)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(f => f.ProjectId);
        
        builder.Property(t => t.Version)
            .IsRequired()
            .ValueGeneratedOnUpdate()
            .IsConcurrencyToken();
        
        builder.Property(p => p.ArchivedAt)
            .IsRequired(false);
        
        builder.Property(p => p.ArchiveSource)
            .IsRequired(false);

        builder.HasMany(it => it.AuthorsList)
            .WithMany(it => it.FeaturesList);
    }
}