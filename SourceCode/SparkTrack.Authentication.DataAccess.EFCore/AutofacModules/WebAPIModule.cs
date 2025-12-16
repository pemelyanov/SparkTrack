namespace SparkTrack.Authentication.DataAccess.EFCore.AutofacModules;

using Autofac;
using Repositories;

public class WebAPIModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterGeneric(typeof(RefreshTokensRepository<>))
            .As(typeof(RefreshTokensRepository<>))
            .InstancePerLifetimeScope();
    }
}