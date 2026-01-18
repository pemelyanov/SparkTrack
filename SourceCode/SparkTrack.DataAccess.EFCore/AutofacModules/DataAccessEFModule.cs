namespace SparkTrack.DataAccess.EFCore.AutofacModules;

using Authentication.DataAccess.EFCore;
using Autofac;
using Repositories;

public class DataAccessEFModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SparkTrackDbContext>().AsSelf().As<RefreshTokenDbContext<Guid>>().InstancePerLifetimeScope();
        
        builder.RegisterType<FeaturesRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<ProjectsRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<UsersRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<CommentsRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<SubTasksRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<PaymentBillsRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();
    }
}