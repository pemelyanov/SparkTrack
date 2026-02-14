namespace SparkTrack.DataAccess.EFCore.Configuration;

using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PaymentDataConfiguration : IEntityTypeConfiguration<PaymentData>
{
    public void Configure(EntityTypeBuilder<PaymentData> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        builder.HasOne(t => t.Admin)
            .WithMany()
            .HasForeignKey(it => it.AdminId)
            .IsRequired();

        builder.HasOne(it => it.Task)
            .WithMany(it => it.Payments)
            .HasForeignKey(it => it.TaskId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(f => f.CreatedAt)
            .IsRequired();
        
        builder.Property(t => t.PaymentType)
            .IsRequired();
        
        builder.Property(t => t.Payment)
            .IsRequired();
    }
}