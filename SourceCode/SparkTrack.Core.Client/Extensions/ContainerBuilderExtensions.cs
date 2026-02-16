using Autofac;
using SparkTrack.Core.Client.Services.Configuration;

namespace SparkTrack.Core.Client.Extensions;

public static class ContainerBuilderExtensions
{
    public static void RegisterJsonConfiguration<TConfigurationModel>(this ContainerBuilder builder, string filePath)
        where TConfigurationModel : new()
    {
        builder.RegisterType<JsonConfigurationService<TConfigurationModel>>()
            .AsImplementedInterfaces()
            .WithParameter(new TypedParameter(typeof(string), filePath))
            .SingleInstance();
    }
}