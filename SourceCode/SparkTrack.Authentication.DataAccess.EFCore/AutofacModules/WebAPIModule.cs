namespace SparkTrack.Authentication.DataAccess.EFCore.AutofacModules;

using Autofac;
using Core.Repositories;
using Repositories;

public class AuthenticationDataAccessEFCoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterGeneric(typeof(RefreshTokensRepository<>))
            .As(typeof(IRefreshTokensRepository<>))
            .InstancePerLifetimeScope();
    }
}