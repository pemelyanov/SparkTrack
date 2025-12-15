namespace SparkTrack.DataAccess.EFCore.Configuration;

using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FileDataConfiguration : IEntityTypeConfiguration<FileData>
{
    public virtual void Configure(EntityTypeBuilder<FileData> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(f => f.Link)
            .IsRequired()
            .HasMaxLength(1000);
    }
}
