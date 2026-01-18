namespace SparkTrack.DataAccess.EFCore.Configuration;

using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserDataConfiguration : IEntityTypeConfiguration<UserData>
{
    public virtual void Configure(EntityTypeBuilder<UserData> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(150);
        
        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.TelegramTag)
            .IsRequired(false);
    }
}