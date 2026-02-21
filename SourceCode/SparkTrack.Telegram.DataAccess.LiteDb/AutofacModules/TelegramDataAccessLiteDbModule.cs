namespace SparkTrack.Telegram.DataAccess.LiteDb.AutofacModules;

using Autofac;
using DatabaseProvider;
using Microsoft.Extensions.Configuration;
using Repositories;

public class TelegramDataAccessLiteDbModule(IConfiguration configuration) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<LiteDatabaseProvider>()
            .WithParameters(
                [
                    new NamedParameter("databasePath", configuration["TelegramBot:LiteDb:DatabasePath"]),
                    new NamedParameter("password", configuration["TelegramBot:LiteDb:Password"])
                ]
            )
            .As<ILiteDatabaseProvider>()
            .SingleInstance();

        builder.RegisterType<TelegramUsersRepository>()
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}