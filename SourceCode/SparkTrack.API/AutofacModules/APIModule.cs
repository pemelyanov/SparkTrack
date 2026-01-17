namespace SparkTrack.API.AutofacModules;

using Autofac;
using API;
using Core.Client.Services.Configuration;
using Data;
using Interceptors;
using Services.Authorization;
using Services.Comments;
using Services.Features;
using Services.Files;
using Services.Projects;
using Services.SubTasks;
using Services.Users;
using System.Reflection;
using Module = Autofac.Module;

public class APIModule(string apiBaseUrl, string tokensConfigPath) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SparkHttpClient>().As<HttpClient>().WithParameter(new TypedParameter(typeof(string), apiBaseUrl));
        builder.RegisterType<RetryAuthHandler>();
        
        RegisterAllClientsFromAssembly<ClientBase>(builder, typeof(APIModule).Assembly);

        builder.RegisterType<FeaturesService>().AsImplementedInterfaces();
        builder.RegisterType<JsonConfigurationService<TokensConfiguration>>()
            .AsImplementedInterfaces()
            .WithParameter(new TypedParameter(typeof(string), tokensConfigPath));
        
        builder.RegisterType<AuthorizationService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UsersService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ProjectsService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<FilesService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<CommentsService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SubTasksService>().AsImplementedInterfaces().SingleInstance();
    }

    private void RegisterAllClientsFromAssembly<TClientBase>(ContainerBuilder builder, Assembly assembly)
    {
        builder.RegisterAssemblyTypes(assembly)
            .Where(it => it.IsAssignableTo(typeof(TClientBase)));

        builder
            .RegisterGeneric(typeof(ClientWrapper<>))
            .AsSelf();
    }
}