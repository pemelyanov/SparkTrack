using SparkTrack.Core.Client.Extensions;

namespace SparkTrack.API.AutofacModules;

using Autofac;
using API;
using Interceptors;
using Services.Authorization;
using Services.Comments;
using Services.Features;
using Services.Files;
using Services.PaymentBills;
using Services.Projects;
using Services.SubTasks;
using Services.Users;
using Core.Client.Data;
using Module = Autofac.Module;

public class APIModule(string apiBaseUrl, string tokensConfigPath) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SparkHttpClient>().As<HttpClient>().WithParameter(new TypedParameter(typeof(string), apiBaseUrl));
        builder.RegisterType<RetryAuthHandler>();
        
        RegisterAllClientsFromAssembly(builder);

        builder.RegisterType<FeaturesService>().AsImplementedInterfaces();
        builder.RegisterJsonConfiguration<TokensConfiguration>(tokensConfigPath);
        
        builder.RegisterType<AuthorizationService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<UsersService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ProjectsService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<FilesService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<CommentsService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SubTasksService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<PaymentBillsService>().AsImplementedInterfaces().SingleInstance();
    }

    private void RegisterAllClientsFromAssembly(ContainerBuilder builder)
    {
        builder
            .RegisterGeneric(typeof(ClientWrapper<>))
            .AsSelf();
    }
}