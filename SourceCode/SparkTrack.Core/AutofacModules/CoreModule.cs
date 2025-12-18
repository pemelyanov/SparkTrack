namespace SparkTrack.Core.AutofacModules;

using Autofac;
using Services.Authorization;
using Services.Features;
using Services.PasswordHasher;
using Services.Projects;

public class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<FeaturesService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<ProjectsService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<AuthorizationService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<PasswordHasher>().AsImplementedInterfaces().InstancePerLifetimeScope();
    }
}