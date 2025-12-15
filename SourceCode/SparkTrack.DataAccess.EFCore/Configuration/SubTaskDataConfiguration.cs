namespace SparkTrack.DataAccess.EFCore.Configuration;

using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SubTaskDataConfiguration : IEntityTypeConfiguration<SubTaskData>
{
    public virtual void Configure(EntityTypeBuilder<SubTaskData> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(300);
        
        builder.Property(t => t.ExecutorEmployeeId)
            .IsRequired();
        
        builder.Property(t => t.Cost)
            .HasPrecision(18, 2) // Для денежных значений
            .IsRequired();
        
        builder.Property(t => t.IsCompleted)
            .IsRequired();
        
        builder.Property(t => t.OnPayment)
            .IsRequired();
        
        // Связь с UserData
        builder.HasOne(t => t.ExecutorEmployee)
            .WithMany()
            .HasForeignKey(t => t.ExecutorEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(t => t.ExecutorEmployeeId);
        builder.HasIndex(t => t.IsCompleted);
    }
}
