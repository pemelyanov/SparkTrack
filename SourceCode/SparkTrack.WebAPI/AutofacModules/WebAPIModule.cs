using SparkTrack.Core.Shared.Eventing;
using SparkTrack.WebAPI.BackgroundHandlers.Telegram;
using SparkTrack.WebAPI.BackgroundServices;
using SparkTrack.WebAPI.Events;

namespace SparkTrack.WebAPI.AutofacModules;

using Autofac;
using DataStore;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Middlewares;
using NLog;
using Services.Files;
using Services.JwtAuthorization;
using Telegram.Core.AutofacModules;
using Telegram.DataAccess.LiteDb.AutofacModules;

public class WebAPIModule(IConfiguration configuration) : Module
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AuthorizationServiceMiddleware>().InstancePerLifetimeScope();
        builder.RegisterType<JwtAuthorizationService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        RegisterGoogleDrive(builder);
        builder.RegisterType<GoogleDriveFilesService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterType<AutofacEventEmitter>().AsImplementedInterfaces().SingleInstance();
        RegisterTelegramBotIfNeeded(builder);
    }

    private void RegisterTelegramBotIfNeeded(ContainerBuilder builder)
    {
        if(!configuration.GetSection("TelegramBot").Exists()) return;

        builder.RegisterModule(new TelegramCoreModule(configuration));
        builder.RegisterModule(new TelegramDataAccessLiteDbModule(configuration));

        builder.RegisterType<TelegramBotBackgroundService>()
            .As<IHostedService>()
            .SingleInstance();

        builder.RegisterGeneric(typeof(TelegramBackgroundEventHandler<>))
            .As(typeof(IEventHandler<>));
    }

    private void RegisterGoogleDrive(ContainerBuilder builder)
    {
        builder.Register(c =>
        {
            var eventEmitter = c.Resolve<IEventEmitter>();
            return new Func<Task<DriveService>>(() => AuthenticateDriveAsync(eventEmitter));
        }).SingleInstance();
    }

    private async Task<DriveService> AuthenticateDriveAsync(IEventEmitter eventEmitter)
    {
        try
        {
            s_logger.Info("Authorizing in google drive...");

            var googleSection = configuration.GetRequiredSection("Google");

            var clientSecrets = await GoogleClientSecrets.FromFileAsync(googleSection["SecretsPath"]);

            var flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets.Secrets,
                    Scopes = new[]
                    {
                        DriveService.Scope.Drive
                    },
                    DataStore = new ThreadSafeFileDataStore(googleSection["FileStorePath"])
                }
            );

            var user = googleSection["User"];

            var credential = new UserCredential(
                flow,
                user,
                await flow.LoadTokenAsync(user, CancellationToken.None)
            );

            return new DriveService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "FanatikiLauncher"
                }
            );
        }
        catch (Exception e)
        {
            s_logger.Error(e, "Google Drive authenticating error:");
            await eventEmitter.RaiseAsync(new GoogleAuthenticationExceptionEvent(e));
            throw;
        }
    }

    // private static void RegisterTelegramEventHandlers(ContainerBuilder builder, Assembly assembly)
    // {
    //     // Находим все типы, реализующие ITelegramEventHandler<>
    //     var handlerTypes = assembly.GetTypes()
    //         .Where(t => t.IsClass && !t.IsAbstract)
    //         .SelectMany(t => t.GetInterfaces()
    //             .Where(i => i.IsGenericType && 
    //                         i.GetGenericTypeDefinition() == typeof(ITelegramEventHandler<>))
    //             .Select(i => new
    //             {
    //                 Type = t,
    //                 EventInterface = i
    //             }))
    //         .ToList();
    //
    //     foreach (var handler in handlerTypes)
    //     {
    //         // Регистрируем как IEventHandler<TEvent> для конкретного типа события
    //         var eventHandlerInterface = typeof(IEventHandler<>)
    //             .MakeGenericType(handler.EventInterface.GetGenericArguments()[0]);
    //         
    //         // Регистрируем как IHostedService
    //         builder.RegisterType(handler.Type)
    //             .As<IHostedService>()
    //             .As(eventHandlerInterface)
    //             .SingleInstance();
    //     }
    // }
}