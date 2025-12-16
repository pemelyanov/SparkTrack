namespace SparkTrack.WebAPI.AutofacModules;

using Autofac;
using Middlewares;

public class WebAPIModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AuthorizationServiceMiddleware>().InstancePerLifetimeScope();
    }
}