namespace SparkTrack.Telegram.Core.AutofacModules;

using Autofac;
using EventHandlers;
using Extensions;
using Microsoft.Extensions.Configuration;
using Repositories;
using Services;

public class TelegramCoreModule(IConfiguration configuration, HashSet<Type> handlingEvents) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterDecorator<CachingUsersRepository, ITelegramUsersRepository>();

        var tokenFile = configuration["TelegramBot:TokenPath"];

        if (string.IsNullOrEmpty(tokenFile))
            throw new InvalidOperationException("Specify path to file with bot token in configuration");

        var token = File.ReadAllText(tokenFile);

        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException("Specify token in token file");
        
        builder.RegisterType<TelegramBotService>()
            .WithParameters([new NamedParameter("botToken", token)])
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterAssemblyAssignableGenericWithArguments(
                GetType().Assembly,
                typeof(ITelegramEventHandler<>),
                handlingEvents
            )
            .InstancePerLifetimeScope();
    }
}