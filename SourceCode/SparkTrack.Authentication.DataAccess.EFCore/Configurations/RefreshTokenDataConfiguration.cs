namespace SparkTrack.Authentication.DataAccess.EFCore.Configurations;

using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RefreshTokenDataConfiguration<TUserKey> : IEntityTypeConfiguration<RefreshTokenData<TUserKey>>
    where TUserKey : notnull
{
    public void Configure(EntityTypeBuilder<RefreshTokenData<TUserKey>> builder)
    {
        // Настройка первичного ключа
        builder.HasKey(e => e.Id);
        
        // Настройка свойств
        builder.Property(e => e.Id)
            .IsRequired();
            
        builder.Property(e => e.UserId)
            .IsRequired();
            
        builder.Property(e => e.Token)
            .IsRequired()
            .HasMaxLength(500)
            .IsUnicode(false);
            
        builder.Property(e => e.TokenHash)
            .IsRequired()
            .HasMaxLength(32)
            .IsUnicode(false);

        builder.Property(e => e.GenerationDate)
            .IsRequired();
        
        // Настройка индексов
        builder.HasIndex(e => e.TokenHash);
        builder.HasIndex(e => e.UserId);
    }
}