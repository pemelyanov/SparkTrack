namespace SparkTrack.DataAccess.EFCore.Configuration;

using Microsoft.EntityFrameworkCore;

public interface IConfigurationApplier
{
    void ApplyConfiguration(ModelBuilder builder);
}