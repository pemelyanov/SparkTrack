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
        
        // Связь с ProjectData
        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(f => f.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Связь с SubTaskData
        builder.HasMany(f => f.TasksList)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        
        // Связь с FileData
        builder.HasMany(f => f.AttachmentsList)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(f => f.ProjectId);
    }
}