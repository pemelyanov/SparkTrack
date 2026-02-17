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

public class WebAPIModule : Module
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AuthorizationServiceMiddleware>().InstancePerLifetimeScope();
        builder.RegisterType<JwtAuthorizationService>().AsImplementedInterfaces().InstancePerLifetimeScope();
        RegisterGoogleDrive(builder);
        builder.RegisterType<GoogleDriveFilesService>().AsImplementedInterfaces().InstancePerLifetimeScope();
    }

    private void RegisterGoogleDrive(ContainerBuilder builder)
    {
        builder.Register(c =>
        {
            var configuration = c.Resolve<IConfiguration>();
            return new Func<Task<DriveService>>(() => AuthenticateDriveAsync(configuration));
        }).SingleInstance();
    }

    private async Task<DriveService> AuthenticateDriveAsync(IConfiguration configuration)
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
            throw;
        }
    }
}