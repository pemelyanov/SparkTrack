namespace SparkTrack.Core.Client.AutofacModules;

using Autofac;
using Services;
using Services.Accounts;
using Shared.Eventing;

public class CoreClientModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AutofacEventEmitter>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<FileSystemAccountsService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<AccountsHandler>().AsImplementedInterfaces().SingleInstance();
    }
}