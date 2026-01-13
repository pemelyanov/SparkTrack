namespace SparkTrack.DataAccess.EFCore.Configuration;

using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AttachmentDataConfiguration : IEntityTypeConfiguration<AttachmentData>
{
    public virtual void Configure(EntityTypeBuilder<AttachmentData> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(f => f.Extension)
            .IsRequired();

        builder.Property(f => f.Size)
            .IsRequired();
        
        builder.Property(f => f.FileId)
            .IsRequired();

        builder.Property(f => f.IsImage)
            .IsRequired();

        builder.Property(f => f.Checksum)
            .IsRequired();
    }
}
