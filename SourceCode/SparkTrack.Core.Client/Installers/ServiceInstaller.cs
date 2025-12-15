namespace SparkTrack.Core.Client.Installers;

using Autofac;
using Services.Authorization;

public static class ServiceInstaller
{
    public static void RegisterClientCoreServices(this ContainerBuilder builder)
    {
        builder.RegisterType<FakeAuthorizationService>().As<IAuthorizationService>().SingleInstance();
    }
}