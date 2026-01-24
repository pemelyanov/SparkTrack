namespace SparkTrack.DataAccess.EFCore.Configuration;

using Core.Shared.Data.Entities;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BonusPaymentDataConfiguration : IEntityTypeConfiguration<BonusPaymentData>
{
    public void Configure(EntityTypeBuilder<BonusPaymentData> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        builder.HasOne(t => t.Admin)
            .WithMany()
            .HasForeignKey(it => it.AdminId)
            .IsRequired();

        builder.HasOne<UserData>()
            .WithMany(it => it.Bonuses)
            .HasForeignKey(it => it.EmployeeId)
            .IsRequired();
        
        builder.Property(f => f.CreatedAt)
            .IsRequired();
        
        builder.Property(t => t.Comment)
            .IsRequired(false);
        
        builder.Property(t => t.Payment)
            .IsRequired();
    }
}