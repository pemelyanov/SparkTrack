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
        builder.RegisterType<FeaturesService>().AsImplementedInterfaces();
        builder.RegisterType<ProjectsService>().AsImplementedInterfaces();
        builder.RegisterType<AuthorizationService>().AsImplementedInterfaces();
        builder.RegisterType<PasswordHasher>().AsImplementedInterfaces();
    }
}