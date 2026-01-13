namespace SparkTrack.Core.AutofacModules;

using Autofac;
using Seeding;
using Seeding.Development;
using Services.Authorization;
using Services.Features;
using Services.Files;
using Services.PasswordHasher;
using Services.Projects;
using Services.Users;

public class CoreModule(bool isDevelopment) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<FeaturesService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<ProjectsService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<AuthorizationService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<PasswordHasher>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<UsersService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<FileSystemFilesService>().AsImplementedInterfaces().InstancePerLifetimeScope();

        RegisterSeeders(builder);
    }

    private void RegisterSeeders(ContainerBuilder builder)
    {
        RegisterSeeder<DefaultGodSeeder>(builder);
        RegisterDevSeeder<AdminsSeeder>(builder);
        RegisterDevSeeder<EmployeesSeeder>(builder);
        RegisterDevSeeder<ProjectsSeeder>(builder);
        RegisterDevSeeder<FeaturesSeeder>(builder);
    }
    
    private void RegisterSeeder<TSeeder>(ContainerBuilder builder) where TSeeder : IDataSeeder
    {
        builder.RegisterType<TSeeder>().As<IDataSeeder>();
    }
    
    private void RegisterDevSeeder<TSeeder>(ContainerBuilder builder) where TSeeder : IDataSeeder
    {
        if(!isDevelopment) return;
        
        RegisterSeeder<TSeeder>(builder);
    }
}