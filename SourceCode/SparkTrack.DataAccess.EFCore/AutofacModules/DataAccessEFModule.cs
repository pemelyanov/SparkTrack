namespace SparkTrack.DataAccess.EFCore.AutofacModules;

using Autofac;
using Repositories;

public class DataAccessEFModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SparkTrackDbContext>().InstancePerLifetimeScope();
        
        builder.RegisterType<FeaturesRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<ProjectsRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
    }
}