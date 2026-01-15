namespace SparkTrack.DataAccess.EFCore.Configuration;

using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CommentDataConfiguration : IEntityTypeConfiguration<CommentData>
{
    public virtual void Configure(EntityTypeBuilder<CommentData> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(f => f.Text)
            .IsRequired();
        
        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(it => it.UserId);
        
        builder.HasOne(f => f.Feature)
            .WithMany()
            .HasForeignKey(it => it.FeatureId);
        
        // Связь с AttachmentData
        builder.HasMany(f => f.AttachmentsList)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(f => f.CreatedAt)
            .IsRequired();
        
        builder.Property(f => f.EditedAt)
            .IsRequired(false);
    }
}