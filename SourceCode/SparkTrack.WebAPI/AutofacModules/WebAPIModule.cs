using SparkTrack.Core.AutofacModules;
using SparkTrack.Core.Shared.Eventing;
using SparkTrack.WebAPI.BackgroundHandlers.Telegram;
using SparkTrack.WebAPI.BackgroundServices;
using SparkTrack.WebAPI.Events;

namespace SparkTrack.WebAPI.AutofacModules;

using Autofac;
using Core.Events;
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
using Telegram.Core.Extensions;
using Telegram.DataAccess.LiteDb.AutofacModules;

public class WebAPIModule(IConfiguration configuration) : Module
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AuthorizationServiceMiddleware>().InstancePerLifetimeScope();
        builder.RegisterType<JwtAuthorizationService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        if(!TryRegisterGoogleDrive(builder)) builder.RegisterFileSystemFileService();
        builder.RegisterType<AutofacEventEmitter>().AsImplementedInterfaces().SingleInstance();
        TryRegisterTelegramBot(builder);
    }

    private void TryRegisterTelegramBot(ContainerBuilder builder)
    {
        if (!configuration.GetSection("TelegramBot").Exists()) return;

        HashSet<Type> handlingEvents =
        [
            typeof(FeatureCreatedEvent), typeof(FeatureUpdatedEvent), typeof(FeatureDeletedEvent),
            typeof(SubTaskCompletedEvent)
        ];

        builder.RegisterModule(new TelegramCoreModule(configuration, handlingEvents));
        builder.RegisterModule(new TelegramDataAccessLiteDbModule(configuration));

        builder.RegisterType<TelegramBotBackgroundService>()
            .As<IHostedService>()
            .SingleInstance();

        builder.RegisterGenericTypes(
            typeof(TelegramBackgroundEventHandler<>),
            typeof(IEventHandler<>),
            handlingEvents,
            registration => registration.As<IHostedService>().SingleInstance()
        );
    }

    private bool TryRegisterGoogleDrive(ContainerBuilder builder)
    {
        var googleSection = configuration.GetSection("Google");
        
        if(!googleSection.Exists()) return false;
        
        builder.Register(c =>
                {
                    var eventEmitter = c.Resolve<IEventEmitter>();
                    return new Func<Task<DriveService>>(() => AuthenticateDriveAsync(eventEmitter, googleSection));
                }
            )
            .SingleInstance();
        
        builder.RegisterType<GoogleDriveFilesService>().AsImplementedInterfaces().InstancePerLifetimeScope();

        return true;
    }

    private async Task<DriveService> AuthenticateDriveAsync(IEventEmitter eventEmitter, IConfigurationSection googleSection)
    {
        try
        {
            s_logger.Info("Authorizing in google drive...");

            

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
}