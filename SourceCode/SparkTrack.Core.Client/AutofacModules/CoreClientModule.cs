namespace SparkTrack.Core.Client.AutofacModules;

using Autofac;
using Shared.Eventing;

public class CoreClientModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AutofacEventEmitter>().AsImplementedInterfaces().SingleInstance();
    }
}