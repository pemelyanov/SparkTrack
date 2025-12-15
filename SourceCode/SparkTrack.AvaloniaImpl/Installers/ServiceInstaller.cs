namespace SparkTrack.AvaloniaImpl.Installers;

using Autofac;
using Services.NavigationListResolver;
using SparkTrack.Core.Client.Services.Authorization;

public static class ServiceInstaller
{
    public static void RegisterAvaloniaServices(this ContainerBuilder builder)
    {
        builder.RegisterType<NavigationListResolver>().As<INavigationListResolver>().SingleInstance();
    }
}