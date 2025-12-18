namespace SparkTrack.WebAPI.AutofacModules;

using Autofac;
using Middlewares;
using Services.JwtAuthorization;

public class WebAPIModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AuthorizationServiceMiddleware>().InstancePerLifetimeScope();
        builder.RegisterType<JwtAuthorizationService>().AsImplementedInterfaces().InstancePerLifetimeScope();
    }
}